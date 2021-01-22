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
    /// Firmware version service manager.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IFirmwareVersionServiceManager" />
    /// <seealso cref="DeviceGitConnectionOptions" />
    public class FirmwareVersionServiceManager : Manager, IFirmwareVersionServiceManager, IServiceManager<FirmwareVersionGitConnectionOptions>
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly IDeviceServiceManager _deviceServiceManager;
        private readonly FirmwareVersionGitConnectionOptions _firmwareVersionGitConnectionOptions;
        private readonly ILogger<FirmwareVersionServiceManager> _logger;
        private const string Prefix = nameof(FirmwareVersionServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceServiceManager">The device service manager.</param>
        /// <param name="firmwareVersionGitConnectionOptions">The firmware version git connection options.</param>
        public FirmwareVersionServiceManager(ILogger<FirmwareVersionServiceManager> logger, IGitRepositoryManager gitRepoManager, IDeviceServiceManager deviceServiceManager, FirmwareVersionGitConnectionOptions firmwareVersionGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(deviceServiceManager, nameof(deviceServiceManager));
            EnsureArg.IsNotNull(firmwareVersionGitConnectionOptions, nameof(firmwareVersionGitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _deviceServiceManager = deviceServiceManager;
            _firmwareVersionGitConnectionOptions = firmwareVersionGitConnectionOptions;

            SetGitRepoConnection();
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync(string deviceType)
        {
            var firmwareVersions = new List<string>();
            var gitUrl = await _deviceServiceManager.GetFirmwareGitUrlAsync(deviceType).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(gitUrl))
            {
                _firmwareVersionGitConnectionOptions.GitRemoteLocation = gitUrl;
                _gitRepoManager.SetConnectionOptions(_firmwareVersionGitConnectionOptions);

                _logger.LogInformation($"{Prefix}: Cloning github repository.");
                await _gitRepoManager.CloneRepositoryAsync().ConfigureAwait(false);
                _logger.LogInformation($"{Prefix}: Github repository cloning is successful.");

                _logger.LogInformation(
                    $"{Prefix} method name: {nameof(GetAllFirmwareVersionsAsync)}: Getting list of all firmware versions for deviceType {deviceType}.");
                firmwareVersions = await _gitRepoManager.GetAllTagNamesAsync().ConfigureAwait(false);
            }

            return firmwareVersions;
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

            _firmwareVersionGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _firmwareVersionGitConnectionOptions.GitLocalFolder);
        }
    }
}
