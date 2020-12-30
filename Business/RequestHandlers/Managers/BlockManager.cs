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
        /// <returns>list of blocks.</returns>
        public async Task<object> GetBlocksAsObjectAsync()
        {
            var gitConnectionOptions = (DeviceGitConnectionOptions)_repoManager.GetConnectionOptions();
            
            await _repoManager.CloneRepositoryAsync().ConfigureAwait(false);

            var directory = new DirectoryInfo(gitConnectionOptions.BlockConfig);
            var blocks = GetListOfBlocks(directory);

            return new { blocks };
        }

        /// <summary>
        /// Gets the list of blocks.
        /// </summary>
        /// <param name="blockConfigDirectory">The block configuration directory.</param>
        /// <returns></returns>
        public List<BlockJsonModel> GetListOfBlocks(DirectoryInfo blockConfigDirectory)
        {
            var blocks = new List<BlockJsonModel>();
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();
            var filesInDirectory = blockConfigDirectory.EnumerateFiles();
            int index = 1;

            for (int lIndex = 0; lIndex < filesInDirectory.Count(); lIndex++)
            {
                string content = File.ReadAllText(filesInDirectory.ElementAt(lIndex).FullName);

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

                    var jsonMdoel = new BlockJsonModel() { Id = index, Type = name, Tag = string.Empty, Args = args };
                    blocks.Add(jsonMdoel);

                    index++;
                }
            }

            return blocks;
        }

        #endregion
    }
}