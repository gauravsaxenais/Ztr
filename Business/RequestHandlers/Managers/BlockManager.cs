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
            var blocks = await GetListOfBlocksAsync().ConfigureAwait(false);

            return new {blocks};
        }

        /// <summary>
        /// Gets the list of blocks asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<BlockJsonModel>> GetListOfBlocksAsync()
        {
            var filesInDirectory = await _blockServiceManager.GetAllBlockFilesAsync().ConfigureAwait(false);

            var data = await BatchProcessBlockFilesAsync(filesInDirectory).ConfigureAwait(false);
            
            return data.ToList();
        }

        private async Task<List<BlockJsonModel>> ProcessBlockFileAsync(IEnumerable<FileInfo> filesInDirectory)
        {
            var blocks = new List<BlockJsonModel>();
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();

            var inDirectory = filesInDirectory as FileInfo[] ?? filesInDirectory.ToArray();
            for (var lIndex = 0; lIndex < inDirectory.Count(); lIndex++)
            {
                var content = await File.ReadAllTextAsync(inDirectory.ElementAt(lIndex).FullName);

                var arguments = Toml.ReadString<BlockReadModel>(content, tomlSettings);
                var name = Path.GetFileNameWithoutExtension(inDirectory.ElementAt(lIndex).Name);

                if (arguments?.Arguments != null && arguments.Arguments.Any())
                {
                    var args = arguments.Arguments.Select((data, index) => new NetworkArgumentReadModel
                    {
                        Id = index + 1,
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
            }

            return blocks;
        }

        private async Task<IEnumerable<BlockJsonModel>> BatchProcessBlockFilesAsync(IEnumerable<FileInfo> models)
        {
            var batchSize = 4;
            var listOfRequests = new List<Task<List<BlockJsonModel>>>();

            var fileModels = models.ToList();
            for (var skip = 0; skip <= fileModels.Count(); skip += batchSize)
            {
                var model = fileModels.Skip<FileInfo>(skip).Take<FileInfo>(batchSize);
                listOfRequests.Add(ProcessBlockFileAsync(model));
            }

            // This will run all the calls in parallel to gain some performance
            var allFinishedTasks = await Task.WhenAll(listOfRequests).ConfigureAwait(false);

            var data = allFinishedTasks.SelectMany(x => x);

            // fix the indexes
            var blockFiles = data.ToList();
            
            FixIndex(blockFiles);

            return blockFiles;
        }

        private void FixIndex(IReadOnlyList<BlockJsonModel> listOfData)
        {
            for (var index = 0; index < listOfData.Count(); index++)
            {
                listOfData[index].Id = index;
            }
        }

        #endregion
    }
}