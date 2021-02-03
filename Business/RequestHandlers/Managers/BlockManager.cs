namespace Business.RequestHandlers.Managers
{
    using Business.Common.Models;
    using Business.GitRepository.Interfaces;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using Nett;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// Parses a toml file and returns arguments for a block
    /// among others.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IBlockManager" />
    public class BlockManager : Manager, IBlockManager
    {
        #region Private Variables
        private readonly IBlockServiceManager _blockServiceManager;
        private readonly ILogger<BlockManager> _logger;
        private const string Prefix = nameof(BlockManager);
        #endregion

        #region Constructors     

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="blockServiceManager">The block service manager.</param>
        public BlockManager(ILogger<BlockManager> logger, IBlockServiceManager blockServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(blockServiceManager, nameof(blockServiceManager));

            _blockServiceManager = blockServiceManager;
            _logger = logger;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the blocks asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<object> GetBlocksAsync(string firmwareVersion, string deviceType)
        {
            _logger.LogInformation($"{Prefix}: methodName: {nameof(GetBlocksAsync)} Getting list of blocks for {firmwareVersion} and {deviceType}.");

            // clone repo here.
            await _blockServiceManager.CloneGitRepoAsync().ConfigureAwait(false);

            // read default values from toml file defaults.toml
            var defaultValueFromTomlFile =
                await _blockServiceManager.GetDefaultTomlFileContentAsync(firmwareVersion, deviceType).ConfigureAwait(false);

            var blocks = await GetBlocksAsync(defaultValueFromTomlFile).ConfigureAwait(false);

            return new { blocks };
        }

        /// <summary>
        /// Gets the list of blocks asynchronous.
        /// </summary>
        /// <param name="configTomlFileContent">Content of the configuration toml file.</param>
        /// <returns></returns>
        public async Task<List<BlockJsonModel>> GetBlocksFromFileAsync(string configTomlFileContent)
        {
            var blocks = await GetBlocksAsync(configTomlFileContent);

            return blocks;
        }

        private async Task<List<BlockJsonModel>> GetBlocksAsync(string configTomlFileContent)
        {
            var blocksFromGitRepository = await BatchProcessBlockFilesAsync().ConfigureAwait(false);

            var dataFromFile = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(configTomlFileContent);
            dataFromFile.Network.TryGetValue("blocks", out var blocksContent);
            var blocksFromFile = new List<BlockJsonModel>();

            if (blocksContent is Dictionary<string, object>[] dictionaries)
            {
                foreach (var dictionary in dictionaries)
                {
                    dictionary.TryGetValue("type", out var type);
                    dictionary.TryGetValue("tag", out var tag);
                    dictionary.TryGetValue("args", out var argument);

                    var tempBlock = blocksFromGitRepository.FirstOrDefault(x => x.Type == (string)type);

                    if (tempBlock != null)
                    {
                        var block = (BlockJsonModel)tempBlock.Clone();

                        var arguments = block.Args;
                        if (argument is Dictionary<string, object> args)
                        {
                            foreach (var (key, value) in args)
                            {
                                var updatedArgument = arguments.FirstOrDefault(x => x.Name == key);

                                if (updatedArgument != null)
                                {
                                    updatedArgument.Value = (string)value;
                                }
                            }
                        }

                        blocksFromFile.Add(block);
                    }
                }
            }

            FixIndex(blocksFromFile);
            return blocksFromFile;
        }

        /// <summary>
        /// Processes the block file asynchronous.
        /// </summary>
        /// <param name="filesData">The files data.</param>
        /// <returns></returns>
        private async Task<List<BlockJsonModel>> ProcessBlockFileAsync(IDictionary<string, string> filesData)
        {
            var blocks = new List<BlockJsonModel>();
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();

            foreach (var data in filesData)
            {
                var blockReadModel = Toml.ReadString<BlockReadModel>(data.Value, tomlSettings);
                var blockFileName = Path.GetFileNameWithoutExtension(data.Key);

                var blockTask = GetBlockAsync(blockReadModel, tag: string.Empty, blockFileName);
                var modulesTask = GetModulesAsync(blockReadModel);

                await Task.WhenAll(blockTask, modulesTask);

                var block = blockTask.Result;
                block.Modules.AddRange(modulesTask.Result);

                blocks.Add(block);
            }

            return blocks;
        }

        /// <summary>
        /// Batches the process block files asynchronous.
        /// </summary>
        /// <returns></returns>
        private async Task<List<BlockJsonModel>> BatchProcessBlockFilesAsync()
        {
            var batchSize = 4;

            var blockFiles
                = await _blockServiceManager.GetAllBlockFilesAsync().ConfigureAwait(false);

            var listOfRequests = new List<Task<List<BlockJsonModel>>>();

            var fileModels = blockFiles.ToList();
            for (var skip = 0; skip <= fileModels.Count(); skip += batchSize)
            {
                var files = fileModels.Skip(skip).Take(batchSize).ToList();
                var listOfData = await FileReaderExtensions.ReadContentsAsync(files);

                listOfRequests.Add(ProcessBlockFileAsync(listOfData));
            }

            // This will run all the calls in parallel to gain some performance
            var allFinishedTasks = await Task.WhenAll(listOfRequests).ConfigureAwait(false);

            var blocks = allFinishedTasks.SelectMany(x => x)
                                                        .OrderBy(item => item.Type)
                                                        .ToList();

            FixIndex(blocks);

            return blocks;
        }

        /// <summary>
        /// Gets the modules for blocks async.
        /// </summary>
        /// <returns>list of modules</returns>
        private async Task<IEnumerable<string>> GetModulesAsync(BlockReadModel blockReadModel)
        {
            var modules = new HashSet<string>();
            const char delimiter = '.';

            if (blockReadModel?.Lines != null && blockReadModel.Lines.Any())
            {
                var data = blockReadModel.Lines.SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x));

                foreach (var item in data.Select(item => item.GetFirstFromSplit(delimiter)))
                {
                    if (item.Any(char.IsWhiteSpace))
                    {
                        if (!item.Split(' ').Last().Contains('$'))
                            modules.Add(item.Split(' ').Last());
                    }

                    else
                    {
                        if (!item.Contains('$'))
                            modules.Add(item);
                    }
                }
            }

            return await Task.FromResult(modules);
        }

        /// <summary>
        /// Gets the block asynchronous.
        /// </summary>
        /// <param name="blockReadModel">The block read model.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="blockName">Name of the block.</param>
        /// <returns></returns>
        private async Task<BlockJsonModel> GetBlockAsync(BlockReadModel blockReadModel, string tag, string blockName)
        {
            var jsonModel = new BlockJsonModel() { Type = blockName, Tag = string.IsNullOrWhiteSpace(tag) ? string.Empty : tag };

            if (blockReadModel?.Arguments != null && blockReadModel.Arguments.Any())
            {
                var args = blockReadModel.Arguments.Select((data, index) => new NetworkArgumentReadModel
                {
                    Id = index,
                    Name = data.Name,
                    Label = data.Label,
                    Description = data.Description,
                    DataType = data.Type,
                    Min = data.Min,
                    Max = data.Max,
                    Value = string.IsNullOrWhiteSpace(data.Value) ? string.Empty : data.Value
                }).ToList();

                jsonModel.Args.AddRange(args);
            }

            return await Task.FromResult(jsonModel);
        }

        /// <summary>
        /// Fixes the index.
        /// </summary>
        /// <param name="listOfData">The list of data.</param>
        private static void FixIndex(IReadOnlyList<BlockJsonModel> listOfData)
        {
            for (var index = 0; index < listOfData.Count(); index++)
            {
                listOfData[index].Id = index;
            }
        }

        #endregion
    }
}