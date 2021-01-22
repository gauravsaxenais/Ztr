namespace Business.RequestHandlers.Managers
{
    using Business.GitRepositoryWrappers.Interfaces;
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
        /// <param name="blockServiceManager">The module service manager.</param>
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
        /// Parses the toml files asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<object> GetBlocksAsync()
        {
            _logger.LogInformation($"{Prefix}: methodName: {nameof(GetBlocksAsync)} Getting list of blocks.");

            // clone repo here.
            await _blockServiceManager.CloneGitRepoAsync().ConfigureAwait(false);
            var blocks = await BatchProcessBlockFilesAsync().ConfigureAwait(false);

            return new { blocks };
        }

        /// <summary>
        /// Gets the list of blocks asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<BlockJsonModel>> GetListOfBlocksAsync()
        {
            // clone repo here.
            await _blockServiceManager.CloneGitRepoAsync().ConfigureAwait(false);
            var blocks = await BatchProcessBlockFilesAsync().ConfigureAwait(false);

            return blocks;
        }

        private async Task<List<BlockJsonModel>> ProcessBlockFileAsync(IDictionary<string, string> filesData)
        {
            var blocks = new List<BlockJsonModel>();
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();

            foreach (var data in filesData)
            {
                var blockReadModel = Toml.ReadString<BlockReadModel>(data.Value, tomlSettings);
                var name = Path.GetFileNameWithoutExtension(data.Key);

                var blockTask = GetBlockAsync(blockReadModel, name);
                var modulesTask = GetModulesAsync(blockReadModel);

                await Task.WhenAll(blockTask, modulesTask);

                var block = blockTask.Result;
                block.Modules.AddRange(modulesTask.Result);

                blocks.Add(block);
            }

            return blocks;
        }

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

        private async Task<BlockJsonModel> GetBlockAsync(BlockReadModel blockReadModel, string name)
        {
            var jsonModel = new BlockJsonModel() { Type = name, Tag = string.Empty };

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
                    Max = data.Max
                }).ToList();

                jsonModel.Args.AddRange(args);
            }

            return await Task.FromResult(jsonModel);
        }

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