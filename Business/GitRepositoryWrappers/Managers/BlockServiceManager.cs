namespace Business.GitRepositoryWrappers.Managers
{
    using Business.GitRepository.Interfaces;
    using Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.Models;

    /// <summary>
    /// Block Service Manager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IBlockServiceManager" />
    /// <seealso cref="ModuleBlockGitConnectionOptions" />
    public class BlockServiceManager : Manager, IBlockServiceManager, IServiceManager<ModuleBlockGitConnectionOptions>
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly ModuleBlockGitConnectionOptions _moduleGitConnectionOptions;
        private readonly ILogger<BlockServiceManager> _logger;
        private const string Prefix = nameof(BlockServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        public BlockServiceManager(ILogger<BlockServiceManager> logger, IGitRepositoryManager gitRepoManager, ModuleBlockGitConnectionOptions moduleGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _moduleGitConnectionOptions = moduleGitConnectionOptions;

            SetGitRepoConnection();
        }

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        public void SetGitRepoConnection()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _moduleGitConnectionOptions.BlockConfig = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.BlockConfig);

            _gitRepoManager.SetConnectionOptions(_moduleGitConnectionOptions);
        }

        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<FileInfo>> GetAllBlockFilesAsync()
        {
            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllBlockFilesAsync)} Getting list of all blocks.");
            var blockConfigDirectory = new DirectoryInfo(_moduleGitConnectionOptions.BlockConfig);
            var filesInDirectory = blockConfigDirectory.EnumerateFiles();

            return await Task.FromResult(filesInDirectory);
        }

        /// <summary>
        /// Gets all modules.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<IEnumerable<string>> GetAllModulesAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
