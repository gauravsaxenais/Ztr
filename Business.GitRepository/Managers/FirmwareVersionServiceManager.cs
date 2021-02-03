namespace Business.GitRepository.Managers
{
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// FirmwareVersion Service Manager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IFirmwareVersionServiceManager" />
    /// <seealso cref="IServiceManager" />
    public class FirmwareVersionServiceManager : ServiceManager, IFirmwareVersionServiceManager
    {
        private readonly ILogger<FirmwareVersionServiceManager> _logger;
        private const string Prefix = nameof(FirmwareVersionServiceManager);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="firmwareVersionGitConnection">The firmware version git connection.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceServiceManager">The device service manager.</param>
        public FirmwareVersionServiceManager(ILogger<FirmwareVersionServiceManager> logger, FirmwareVersionGitConnectionOptions firmwareVersionGitConnection, IGitRepositoryManager gitRepoManager, IDeviceServiceManager deviceServiceManager) : base(logger, firmwareVersionGitConnection, gitRepoManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            _logger = logger;
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync()
        {
            _logger.LogInformation(
                $"{Prefix} method name: {nameof(GetAllFirmwareVersionsAsync)}: Getting list of all firmware versions for deviceType.");
            var firmwareVersions = await RepoManager.GetAllTagNamesAsync().ConfigureAwait(false);

            return firmwareVersions;
        }

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        public async Task CloneGitRepoAsync()
        {
            await CloneGitHubRepoAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the git repo URL.
        /// </summary>
        /// <param name="gitUrl">The git URL.</param>
        public void SetGitRepoUrl(string gitUrl)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(gitUrl);

            ConnectionOptions.GitRemoteLocation = gitUrl;
            RepoManager.SetConnectionOptions(ConnectionOptions);
        }
    }
}
