namespace Business.GitRepository.Managers
{
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration;

    public class ServiceManager : Manager, IServiceManager
    {
        private readonly ILogger<ServiceManager> _logger;
        protected string AppPath { get; }
        protected IGitRepositoryManager RepoManager { get; set; }
        protected GitConnectionOptions ConnectionOptions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitConnectionOptions">The device git connection options.</param>
        public ServiceManager(ILogger<ServiceManager> logger, GitConnectionOptions gitConnectionOptions, IGitRepositoryManager repoManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitConnectionOptions, nameof(gitConnectionOptions));
            EnsureArg.IsNotNull(repoManager, nameof(repoManager));

            _logger = logger;
            ConnectionOptions = gitConnectionOptions;
            RepoManager = repoManager;

            AppPath = GetCurrentAppPath();
            SetupDependencies(ConnectionOptions);
            SetConnection(ConnectionOptions);
        }

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        protected async Task CloneGitHubRepoAsync()
        {
            _logger.LogInformation($"Cloning github repository.");
            await RepoManager.InitRepositoryAsync().ConfigureAwait(false);
            _logger.LogInformation($"Github repository cloning is successful.");
        }

        /// <summary>
        /// Sets the connection.
        /// </summary>
        /// <param name="connectionOptions">The connection options.</param>
        public void SetConnection(GitConnectionOptions connectionOptions)
        {
            _logger.LogInformation("Setting git repository connection");          
            ConnectionOptions.GitLocalFolder = Path.Combine(AppPath, connectionOptions.GitLocalFolder);
            RepoManager.SetConnectionOptions(ConnectionOptions);
        }

        protected virtual void SetupDependencies(GitConnectionOptions connectionOptions)
        {
        }

        private string GetCurrentAppPath()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return currentDirectory;
        }
    }
}
