namespace Business.GitRepository.ZTR.M7Config.Business
{
    using EnsureThat;
    using global::ZTR.Framework.Business;
    using global::ZTR.M7Config.Business.Common.Configuration;
    using global::ZTR.M7Config.Business.GitRepository.Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// BlockServiceManager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IBlockServiceManager" />
    /// <seealso cref="IServiceManager" />
    public class BlockServiceManager : IBlockServiceManager
    {
        private readonly ILogger<BlockServiceManager> _logger;
        private readonly IModuleServiceManager _moduleServiceManager;
        private readonly IGitRepositoryManager _repoManager;
        private readonly BlockGitConnectionOptions _blockGitConnectionOptions;
        private const string Prefix = nameof(BlockServiceManager);
        private readonly string TomlFileExtension = ".toml";

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="blockGitConnectionOptions">The block git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public BlockServiceManager(ILogger<BlockServiceManager> logger, BlockGitConnectionOptions blockGitConnectionOptions, IGitRepositoryManager gitRepoManager, IModuleServiceManager moduleServiceManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleServiceManager, nameof(moduleServiceManager));
            EnsureArg.IsNotNull(blockGitConnectionOptions, nameof(blockGitConnectionOptions));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));

            _logger = logger;
            _moduleServiceManager = moduleServiceManager;
            _blockGitConnectionOptions = blockGitConnectionOptions;
            _repoManager = gitRepoManager;
        }

        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        public async Task<List<FileInfo>> GetAllBlockFilesAsync()
        {
            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllBlockFilesAsync)} Getting list of all blocks.");
            var appPath = GlobalMethods.GetCurrentAppPath();
            var blockConfigPath = Path.Combine(appPath, _blockGitConnectionOptions.GitLocalFolder, _blockGitConnectionOptions.TomlConfiguration.TomlConfigFolder);
            var blockConfigDirectory = new DirectoryInfo(blockConfigPath);
            var filesInDirectory = blockConfigDirectory.EnumerateFiles().ToList();

            // return only files with .toml extension.
            var blockFiles = filesInDirectory.Where(item => item.Extension.Compares(TomlFileExtension)).ToList();
            return await Task.FromResult(blockFiles);
        }

        /// <summary>
        /// Sets the connection.
        /// </summary>
        public void SetConnection()
        {
            _logger.LogInformation("Setting git repository connection");
            var appPath = GlobalMethods.GetCurrentAppPath();
            _blockGitConnectionOptions.GitLocalFolder = Path.Combine(appPath, _blockGitConnectionOptions.GitLocalFolder);
            _repoManager.SetConnectionOptions(_blockGitConnectionOptions);
        }

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task CloneGitRepoAsync()
        {
            _logger.LogInformation($"Cloning github repository for blocks.");
            SetConnection();
            await _repoManager.InitRepositoryAsync().ConfigureAwait(false);
            _logger.LogInformation($"Github repository cloning is successful for blocks.");
        }
    }
}
