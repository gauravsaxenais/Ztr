namespace Business.RequestHandlers.Managers
{
    using Business.GitRepository.Interfaces;
    using Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.Models;

    /// <summary>
    /// Returns device information.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDeviceTypeManager" />
    public class DeviceTypeManager : Manager, IDeviceTypeManager
    {
        private readonly IDeviceServiceManager _deviceServiceManager;
        private readonly DeviceGitConnectionOptions _devicesGitConnectionOptions;
        private readonly ILogger _logger;
        private const string Prefix = nameof(DeviceTypeManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTypeManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        /// <param name="deviceServiceManager">The device service manager.</param>
        public DeviceTypeManager(ILogger<DeviceTypeManager> logger, DeviceGitConnectionOptions deviceGitConnectionOptions, IDeviceServiceManager deviceServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(deviceServiceManager, nameof(deviceServiceManager));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));

            _deviceServiceManager = deviceServiceManager;
            _logger = logger;
            _devicesGitConnectionOptions = deviceGitConnectionOptions;

            SetGitRepoConnection();
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetAllDevicesAsync)}: Getting list of all devices.");
            await _deviceServiceManager.CloneGitHubRepoAsync().ConfigureAwait(false);
            var listOfDevices = await _deviceServiceManager.GetAllDevicesAsync(_devicesGitConnectionOptions.DeviceToml);
            
            return listOfDevices;
        }

        /// <summary>
        /// Gets the firmware git URL asynchronous.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<string> GetFirmwareGitUrlAsync(string deviceType)
        {
            object url = null;
            var dictionaryDevices = await _deviceServiceManager.GetListOfDevicesAsync(_devicesGitConnectionOptions.DeviceToml).ConfigureAwait(false);

            var device =
                dictionaryDevices.FirstOrDefault(d => d.TryGetValue("name", out object value)
                                                      && value is string i
                                                      && string.Equals(i, deviceType,
                                                          StringComparison.OrdinalIgnoreCase));
            device?.TryGetValue("url", out url);

            dictionaryDevices.Clear();
            return url != null ? url.ToString() : string.Empty;
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

            _devicesGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _devicesGitConnectionOptions.GitLocalFolder);
            _devicesGitConnectionOptions.DeviceToml = Path.Combine(_devicesGitConnectionOptions.GitLocalFolder, _devicesGitConnectionOptions.DeviceToml);

            _deviceServiceManager.SetGitRepoConnection(_devicesGitConnectionOptions);
        }
    }
}
