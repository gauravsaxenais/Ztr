namespace Business.RequestHandlers.Managers
{
    using Configuration;
    using Business.GitRepository.Interfaces;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.Models;


    /// <summary>
    /// Returns firmware version information.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IFirmwareVersionManager" />
    public class FirmwareVersionManager : Manager, IFirmwareVersionManager
    {
        private readonly IFirmwareVersionServiceManager _firmwareVersionServiceManager;
        private readonly ILogger _logger;
        private const string Prefix = nameof(FirmwareVersionManager);
        private readonly IDeviceTypeManager _deviceTypeManager;
        private readonly FirmwareVersionGitConnectionOptions _firmwareVersionGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="firmwareVersionServiceManager">The firmware version service manager.</param>
        /// <param name="firmwareVersionGitConnection">The firmware version git connection.</param>
        /// <param name="deviceTypeManager">The device type manager.</param>
        public FirmwareVersionManager(ILogger<DeviceTypeManager> logger, IFirmwareVersionServiceManager firmwareVersionServiceManager, FirmwareVersionGitConnectionOptions firmwareVersionGitConnection, IDeviceTypeManager deviceTypeManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(firmwareVersionServiceManager, nameof(firmwareVersionServiceManager));

            _firmwareVersionServiceManager = firmwareVersionServiceManager;
            _logger = logger;
            _deviceTypeManager = deviceTypeManager;
            _firmwareVersionGitConnectionOptions = firmwareVersionGitConnection;

            SetGitRepoConnection();
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync(string deviceType)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(deviceType);

            var gitUrl = await _deviceTypeManager.GetFirmwareGitUrlAsync(deviceType).ConfigureAwait(false);
            _logger.LogInformation($"{Prefix}: methodName: {nameof(GetAllFirmwareVersionsAsync)} Getting list of compatible firmware versions based on a device type {deviceType}.");

            _firmwareVersionGitConnectionOptions.GitRemoteLocation = gitUrl;
            _firmwareVersionServiceManager.SetGitRepoConnection(_firmwareVersionGitConnectionOptions);

            var listFirmwareVersions = await _firmwareVersionServiceManager.GetAllFirmwareVersionsAsync()
                .ConfigureAwait(false);

            return listFirmwareVersions;
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

            _firmwareVersionServiceManager.SetGitRepoConnection(_firmwareVersionGitConnectionOptions);
        }
    }
}
