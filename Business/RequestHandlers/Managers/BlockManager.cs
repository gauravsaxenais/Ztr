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
        public async Task<object> GetBlocksAsObjectAsync()
        {
            _logger.LogInformation($"{Prefix}: methodName: {nameof(GetBlocksAsObjectAsync)} Getting list of blocks.");
            var (blocks, modules) = await BatchProcessBlockFilesAsync().ConfigureAwait(false);

            return new { blocks, modules };
        }

        /// <summary>
        /// Gets the list of blocks asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<BlockJsonModel>> GetListOfBlocksAsync()
        {
            // using discard _ for modules.
            var (blocks, _) = await BatchProcessBlockFilesAsync().ConfigureAwait(false);

            return blocks;
        }

        private async Task<(List<BlockJsonModel> blocks, List<string> modules)> ProcessBlockFileAsync(IEnumerable<FileInfo> filesInDirectory)
        {
            var blocks = new List<BlockJsonModel>();
            var modules = new List<string>();
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();

            var inDirectory = filesInDirectory as FileInfo[] ?? filesInDirectory.ToArray();
            for (var lIndex = 0; lIndex < inDirectory.Count(); lIndex++)
            {
                var content = await File.ReadAllTextAsync(inDirectory.ElementAt(lIndex).FullName);

                var blockReadModel = Toml.ReadString<BlockReadModel>(content, tomlSettings);
                var name = Path.GetFileNameWithoutExtension(inDirectory.ElementAt(lIndex).Name);

                var blocksTask = GetBlocksAsync(blockReadModel, name);
                var modulesTask = GetModulesAsync(blockReadModel);

                await Task.WhenAll(blocksTask, modulesTask);

                blocks.AddRange(blocksTask.Result);
                modules.AddRange(modulesTask.Result);
            }

            return (blocks, modules);
        }

        private async Task<(List<BlockJsonModel> blocks, List<string> modules)> BatchProcessBlockFilesAsync()
        {
            var batchSize = 4;

            var models
                = await _blockServiceManager.GetAllBlockFilesAsync().ConfigureAwait(false);
            var listOfRequests = new List<Task<(List<BlockJsonModel> blocks, List<string> modules)>>();

            var fileModels = models.ToList();
            for (var skip = 0; skip <= fileModels.Count(); skip += batchSize)
            {
                var model = fileModels.Skip<FileInfo>(skip).Take<FileInfo>(batchSize);
                listOfRequests.Add(ProcessBlockFileAsync(model));
            }

            // This will run all the calls in parallel to gain some performance
            var allFinishedTasks = await Task.WhenAll(listOfRequests).ConfigureAwait(false);

            var blocks = allFinishedTasks.SelectMany(x => x.blocks).ToList();

            // remove duplicates.
            var modules = new HashSet<string>(allFinishedTasks.SelectMany(x => x.modules)).ToList();

            FixIndex(blocks);

            return (blocks, modules);
        }

        /// <summary>
        /// Gets the modules for blocks async.
        /// </summary>
        /// <returns>list of modules</returns>
        private async Task<IEnumerable<string>> GetModulesAsync(BlockReadModel blockReadModel)
        {
            var modules = new List<string>();
            const char delimiter = '.';

            if (blockReadModel?.Lines != null && blockReadModel.Lines.Any())
            {
                var data = blockReadModel.Lines.SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x));

                foreach (var item in data.Select(item => item.GetFirstFromSplit(delimiter)))
                {
                    if (item.Any(char.IsWhiteSpace))
                    {
                        if(!item.Split(' ').Last().Contains('$'))
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

        private async Task<IEnumerable<BlockJsonModel>> GetBlocksAsync(BlockReadModel blockReadModel, string name)
        {
            var blocks = new List<BlockJsonModel>();
            if (blockReadModel?.Arguments != null && blockReadModel.Arguments.Any())
            {
                var args = blockReadModel.Arguments.Select((data, index) => new NetworkArgumentReadModel
                {
                    Id = index,
                    Name = data.Name,
                    Label = data.Label,
                    Description = data.Description,
                    DataType = data.DataType,
                    Min = data.Min,
                    Max = data.Max
                }).ToList();

                var jsonModel = new BlockJsonModel() { Type = name, Tag = string.Empty, Args = args };
                blocks.Add(jsonModel);
            }

            return await Task.FromResult(blocks);
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