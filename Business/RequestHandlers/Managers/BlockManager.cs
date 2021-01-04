namespace Business.RequestHandlers.Managers
{
    using System;
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
        private readonly IModuleServiceManager _moduleServiceManager;
        #endregion

        #region Constructors                                
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleServiceManager">The module service manager.</param>
        public BlockManager(ILogger<BlockManager> logger, IModuleServiceManager moduleServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleServiceManager, nameof(moduleServiceManager));

            _moduleServiceManager = moduleServiceManager;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Parses the toml files asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse> GetBlocksAsObjectAsync()
        {
            var prefix = nameof(BlockManager);
            ApiResponse apiResponse = null;

            try
            {
                Logger.LogInformation($"{prefix}: Getting list of blocks.");
                var blocks = await GetListOfBlocksAsync().ConfigureAwait(false);

                apiResponse = new ApiResponse(status: true, data: new {blocks});
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, $"{prefix}: Error occurred while getting list of blocks.");
                apiResponse = new ApiResponse(false, exception.Message, ErrorType.BusinessError, exception);
            }

            return apiResponse;
        }

        /// <summary>
        /// Gets the list of blocks asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<BlockJsonModel>> GetListOfBlocksAsync()
        {
            var filesInDirectory = _moduleServiceManager.GetAllBlockFiles();

            var data = await BatchProcessBlockFiles(filesInDirectory).ConfigureAwait(false);
            
            return data.ToList();
        }

        private async Task<List<BlockJsonModel>> ProcessBlockFileAsync(IEnumerable<FileInfo> filesInDirectory)
        {
            var blocks = new List<BlockJsonModel>();
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            var inDirectory = filesInDirectory as FileInfo[] ?? filesInDirectory.ToArray();
            for (var lIndex = 0; lIndex < inDirectory.Count(); lIndex++)
            {
                var content = await File.ReadAllTextAsync(inDirectory.ElementAt(lIndex).FullName);

                var arguments = Toml.ReadString<BlockReadModel>(content, tomlSettings);
                var name = Path.GetFileNameWithoutExtension(inDirectory.ElementAt(lIndex).Name);

                if (arguments?.Arguments != null && arguments.Arguments.Any())
                {
                    var args = arguments.Arguments.Select((args, index) => new NetworkArgumentReadModel
                    {
                        Id = index + 1,
                        Name = args.Name,
                        Label = args.Label,
                        Description = args.Description,
                        DataType = args.DataType,
                        Min = args.Min,
                        Max = args.Max
                    }).ToList();

                    var jsonModel = new BlockJsonModel() { Type = name, Tag = string.Empty, Args = args };
                    blocks.Add(jsonModel);
                }
            }

            return blocks;
        }

        private async Task<IEnumerable<BlockJsonModel>> BatchProcessBlockFiles(IEnumerable<FileInfo> models)
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