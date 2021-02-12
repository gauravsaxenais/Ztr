namespace Business.GitRepository.Managers
{
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System;
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
        public async Task<IEnumerable<FileInfo>> GetAllBlockFilesAsync()
        {
            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllBlockFilesAsync)} Getting list of all blocks.");
            string blockConfigPath = ((BlockGitConnectionOptions)ConnectionOptions).GitLocalFolder;

            var blockConfigDirectory = new DirectoryInfo(blockConfigPath);
            var filesInDirectory = blockConfigDirectory.EnumerateFiles();

            return await Task.FromResult(filesInDirectory);
        }

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task CloneGitRepoAsync()
        {
            await CloneGitHubRepoAsync().ConfigureAwait(false);
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
