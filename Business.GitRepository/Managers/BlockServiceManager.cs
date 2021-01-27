using Business.Common.Configuration;
using ZTR.Framework.Business.Models;

namespace Business.GitRepository.Managers
{
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// BlockServiceManager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IBlockServiceManager" />
    /// <seealso cref="IServiceManager{GitConnectionOptions}" />
    public class BlockServiceManager : Manager, IBlockServiceManager, IServiceManager<GitConnectionOptions>
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly ILogger<BlockServiceManager> _logger;
        private const string Prefix = nameof(BlockServiceManager);
        private readonly ModuleBlockGitConnectionOptions _moduleGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public BlockServiceManager(ILogger<BlockServiceManager> logger, ModuleBlockGitConnectionOptions moduleGitConnectionOptions, IGitRepositoryManager gitRepoManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;

            _moduleGitConnectionOptions = moduleGitConnectionOptions;
            SetGitRepoConnection(moduleGitConnectionOptions);
        }

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        public void SetGitRepoConnection(ModuleBlockGitConnectionOptions moduleGitConnectionOptions)
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _moduleGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, moduleGitConnectionOptions.GitLocalFolder);
            _moduleGitConnectionOptions.BlockConfig = Path.Combine(currentDirectory, moduleGitConnectionOptions.GitLocalFolder, moduleGitConnectionOptions.BlockConfig);

            _gitRepoManager.SetConnectionOptions(_moduleGitConnectionOptions);
        }

        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<FileInfo>> GetAllBlockFilesAsync()
        {
            string blockConfigPath = _moduleGitConnectionOptions.BlockConfig;
            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllBlockFilesAsync)} Getting list of all blocks.");
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
            _logger.LogInformation($"{Prefix}: Cloning github repository.");
            await _gitRepoManager.CloneRepositoryAsync().ConfigureAwait(false);
            _logger.LogInformation($"{Prefix}: Github repository cloning is successful.");
        }
    }
}
