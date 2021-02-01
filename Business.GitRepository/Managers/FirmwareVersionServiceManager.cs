namespace Business.GitRepository.Managers
{
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration;

    /// <summary>
    /// FirmwareVersion Service Manager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IFirmwareVersionServiceManager" />
    /// <seealso cref="IServiceManager" />
    public class FirmwareVersionServiceManager : Manager, IFirmwareVersionServiceManager, IServiceManager
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly ILogger<FirmwareVersionServiceManager> _logger;
        private const string Prefix = nameof(FirmwareVersionServiceManager);
        private readonly FirmwareVersionGitConnectionOptions _firmwareVersionGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="firmwareVersionGitConnection">The firmware version git connection.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceServiceManager">The device service manager.</param>
        public FirmwareVersionServiceManager(ILogger<FirmwareVersionServiceManager> logger, FirmwareVersionGitConnectionOptions firmwareVersionGitConnection, IGitRepositoryManager gitRepoManager, IDeviceServiceManager deviceServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(deviceServiceManager, nameof(deviceServiceManager));
            EnsureArg.IsNotNull(firmwareVersionGitConnection, nameof(firmwareVersionGitConnection));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _firmwareVersionGitConnectionOptions = firmwareVersionGitConnection;

            SetGitRepoConnection(_firmwareVersionGitConnectionOptions);
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync()
        {
            _logger.LogInformation(
                $"{Prefix} method name: {nameof(GetAllFirmwareVersionsAsync)}: Getting list of all firmware versions for deviceType.");
            var firmwareVersions = await _gitRepoManager.GetAllTagNamesAsync().ConfigureAwait(false);

            return firmwareVersions;
        }

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        public async Task CloneGitHubRepoAsync()
        {
            _logger.LogInformation($"{Prefix}: Cloning github repository.");
            await _gitRepoManager.CloneRepositoryAsync().ConfigureAwait(false);
            _logger.LogInformation($"{Prefix}: Github repository cloning is successful.");
        }

        /// <summary>
        /// Sets the git repo URL.
        /// </summary>
        /// <param name="gitUrl">The git URL.</param>
        public void SetGitRepoUrl(string gitUrl)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(gitUrl);

            _firmwareVersionGitConnectionOptions.GitRemoteLocation = gitUrl;
            _gitRepoManager.SetConnectionOptions(_firmwareVersionGitConnectionOptions);
        }

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        private void SetGitRepoConnection(GitConnectionOptions connectionOptions)
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _firmwareVersionGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _firmwareVersionGitConnectionOptions.GitLocalFolder);
            _gitRepoManager.SetConnectionOptions(connectionOptions);
        }
    }
}
