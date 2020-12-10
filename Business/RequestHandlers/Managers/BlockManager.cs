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
        private readonly BlockGitConnectionOptions _blockGitConnectionOptions;
        #endregion

        #region Constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="blockGitConnectionOptions">The block git connection options.</param>
        public BlockManager(ILogger<ModuleManager> logger, IGitRepositoryManager gitRepoManager, BlockGitConnectionOptions blockGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(blockGitConnectionOptions, nameof(blockGitConnectionOptions));

            _repoManager = gitRepoManager;
            _blockGitConnectionOptions = blockGitConnectionOptions;

            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _blockGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _blockGitConnectionOptions.GitLocalFolder);
            _blockGitConnectionOptions.BlockConfig = Path.Combine(currentDirectory, _blockGitConnectionOptions.GitLocalFolder, _blockGitConnectionOptions.BlockConfig);
            _repoManager.SetConnectionOptions(_blockGitConnectionOptions);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Parses the toml files asynchronous.
        /// </summary>
        /// <returns>list of blocks.</returns>
        public async Task<object> ParseTomlFilesAsync()
        {
            var gitConnectionOptions = (BlockGitConnectionOptions)_repoManager.GetConnectionOptions();
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            await _repoManager.CloneRepositoryAsync();

            var directory = new DirectoryInfo(gitConnectionOptions.BlockConfig);
            var blocks = new List<BlockJsonModel>();

            var filesInDirectory = directory.EnumerateFiles();
            int index = 1;

            for (int lIndex = 0; lIndex < filesInDirectory.Count(); lIndex++)
            {
                string content = File.ReadAllText(filesInDirectory.ElementAt(lIndex).FullName);
                var fileContent = Toml.ReadString(content);

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

            return new { blocks };
        }

        #endregion
    }
}