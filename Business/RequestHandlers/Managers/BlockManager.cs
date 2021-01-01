namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
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
        private readonly IGitRepositoryManager _repoManager;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;
        #endregion

        #region Constructors                        
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        public BlockManager(ILogger<BlockManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions deviceGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));

            _repoManager = gitRepoManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;

            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);
            _deviceGitConnectionOptions.BlockConfig = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.BlockConfig);
            _repoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Parses the toml files asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<object> GetBlocksAsObjectAsync()
        {
            await _repoManager.CloneRepositoryAsync().ConfigureAwait(false);
            var blocks = await GetListOfBlocksAsync().ConfigureAwait(false);

            return new { blocks };
        }

        /// <summary>
        /// Gets the list of blocks.
        /// </summary>
        /// <returns></returns>
        public async Task<List<BlockJsonModel>> GetListOfBlocksAsync()
        {
            var blockConfigDirectory = new DirectoryInfo(_deviceGitConnectionOptions.BlockConfig);
            var filesInDirectory = blockConfigDirectory.EnumerateFiles();

            var data = await BatchProcessBlockFiles(filesInDirectory).ConfigureAwait(false);
            
            return data.ToList();
        }

        private async Task<List<BlockJsonModel>> ProcessBlockFileAsync(IEnumerable<FileInfo> filesInDirectory)
        {
            var blocks = new List<BlockJsonModel>();
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            for (int lIndex = 0; lIndex < filesInDirectory.Count(); lIndex++)
            {
                string content = await File.ReadAllTextAsync(filesInDirectory.ElementAt(lIndex).FullName);

                var arguments = Toml.ReadString<BlockReadModel>(content, tomlSettings);
                var name = Path.GetFileNameWithoutExtension(filesInDirectory.ElementAt(lIndex).Name);

                if (arguments != null && arguments.Arguments != null && arguments.Arguments.Any())
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

            for (var skip = 0; skip <= models.Count(); skip += batchSize)
            {
                var model = models.Skip(skip).Take(batchSize);
                listOfRequests.Add(ProcessBlockFileAsync(model));
            }

            // This will run all the calls in parallel to gain some performance
            var allFinishedTasks = await Task.WhenAll(listOfRequests).ConfigureAwait(false);

            var data = allFinishedTasks.SelectMany(x => x);

            // fix the indexes
            data.Select((item, index) => { item.Id = index; return item; }).ToList();

            return data;
        }

        #endregion
    }
}