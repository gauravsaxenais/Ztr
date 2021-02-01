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
        private readonly IGitConnectionOptions _connectionOptions;
        private readonly ILogger<ServiceManager> _logger;
        private readonly IGitRepositoryManager _repoManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitConnectionOptions">The device git connection options.</param>
        public ServiceManager(ILogger<ServiceManager> logger, IGitConnectionOptions gitConnectionOptions, IGitRepositoryManager repoManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitConnectionOptions, nameof(gitConnectionOptions));
            EnsureArg.IsNotNull(repoManager, nameof(repoManager));

            _logger = logger;
            _connectionOptions = gitConnectionOptions;
            _repoManager = repoManager;
        }

        /// <summary>
        /// Sets the connection.
        /// </summary>
        /// <param name="connectionOptions">The connection options.</param>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        public async Task SetConnection(IGitConnectionOptions connectionOptions)
        {
            _logger.LogInformation("Setting git repository connection");
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _connectionOptions.GitLocalFolder = Path.Combine(currentDirectory, connectionOptions.GitLocalFolder);

            await SetupDependenciesAsync().ConfigureAwait(false);
            _repoManager.SetConnectionOptions(_connectionOptions);
        }

        protected virtual Task SetupDependenciesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
