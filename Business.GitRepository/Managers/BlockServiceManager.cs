namespace Business.GitRepository.Managers
{
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Configuration;

    /// <summary>
    /// BlockServiceManager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IBlockServiceManager" />
    /// <seealso cref="IServiceManager" />
    public class BlockServiceManager : ServiceManager, IBlockServiceManager
    {
        private readonly ILogger<BlockServiceManager> _logger;
        private readonly IModuleServiceManager _moduleServiceManager;
        private const string Prefix = nameof(BlockServiceManager);
        private readonly string TomlFileExtension = ".toml";

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="blockGitConnectionOptions">The block git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public BlockServiceManager(ILogger<BlockServiceManager> logger, BlockGitConnectionOptions blockGitConnectionOptions, IGitRepositoryManager gitRepoManager, IModuleServiceManager moduleServiceManager) : base(logger, blockGitConnectionOptions, gitRepoManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            _logger = logger;
            _moduleServiceManager = moduleServiceManager;
        }

        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        public async Task<List<FileInfo>> GetAllBlockFilesAsync()
        {
            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllBlockFilesAsync)} Getting list of all blocks.");
            var connectionOption = (BlockGitConnectionOptions)ConnectionOptions;
            var blockConfigPath = Path.Combine(AppPath, connectionOption.GitLocalFolder, connectionOption.TomlConfiguration.TomlConfigFolder);

            var blockConfigDirectory = new DirectoryInfo(blockConfigPath);
            var filesInDirectory = blockConfigDirectory.EnumerateFiles().ToList();

            // return only files with .toml extension.
            var blockFiles = filesInDirectory.Where(item => item.Extension.Compares(TomlFileExtension)).ToList();

            return await Task.FromResult(blockFiles);
        }

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task CloneGitRepoAsync()
        {
            _logger.LogInformation($"Cloning github repository for blocks.");
            SetConnection((BlockGitConnectionOptions)ConnectionOptions);
            await CloneGitHubRepoAsync().ConfigureAwait(false);
            _logger.LogInformation($"Github repository cloning is successful for blocks.");
        }

        /// <summary>
        /// Setups the dependencies.
        /// </summary>
        protected override void SetupDependencies(GitConnectionOptions connectionOptions)
        {
            var blockGitConnectionOptions = (BlockGitConnectionOptions)connectionOptions;
            blockGitConnectionOptions.GitLocalFolder = Path.Combine(AppPath, blockGitConnectionOptions.GitLocalFolder);
        }
    }
}
