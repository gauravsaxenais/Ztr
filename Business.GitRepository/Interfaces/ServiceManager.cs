namespace Business.GitRepository.Interfaces
{
    using EnsureThat;
    using Managers;
    using Microsoft.Extensions.Logging;
    using System.IO;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration;

    public class ServiceManager : Manager, IServiceManager
    {
        private readonly GitConnectionOptions _connectionOptions;
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly ILogger<ServiceManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitConnectionOptions">The device git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public ServiceManager(ILogger<ServiceManager> logger, GitConnectionOptions gitConnectionOptions, IGitRepositoryManager gitRepoManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(gitConnectionOptions, nameof(gitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _connectionOptions = gitConnectionOptions;

            SetConnection(_connectionOptions);
        }

        private void SetConnection(GitConnectionOptions connectionOptions)
        {
            _logger.LogInformation("Setting git repository connection");
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _connectionOptions.GitLocalFolder = Path.Combine(currentDirectory, connectionOptions.GitLocalFolder);

            _gitRepoManager.SetConnectionOptions(connectionOptions);
        }
    }
}
