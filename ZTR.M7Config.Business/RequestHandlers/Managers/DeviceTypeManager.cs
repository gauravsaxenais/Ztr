namespace ZTR.M7Config.Business.RequestHandlers.Managers
{
    using ZTR.M7Config.Business.GitRepository.Interfaces;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Returns device information.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDeviceTypeManager" />
    public class DeviceTypeManager : Manager, IDeviceTypeManager
    {
        private readonly IDeviceServiceManager _deviceServiceManager;
        private readonly ILogger _logger;
        private const string Prefix = nameof(DeviceTypeManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTypeManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="deviceServiceManager">The device service manager.</param>
        public DeviceTypeManager(ILogger<DeviceTypeManager> logger, IDeviceServiceManager deviceServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(deviceServiceManager, nameof(deviceServiceManager));
            
            _deviceServiceManager = deviceServiceManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetAllDevicesAsync)}: Getting list of all devices.");
            await _deviceServiceManager.CloneGitRepoAsync().ConfigureAwait(false);
            var listOfDevices = await _deviceServiceManager.GetAllDevicesAsync();
            
            return listOfDevices;
        }

        /// <summary>
        /// Gets the firmware git URL asynchronous.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<string> GetFirmwareGitUrlAsync(string deviceType)
        {
            var url = await _deviceServiceManager.GetFirmwareGitUrlAsync(deviceType).ConfigureAwait(false);
            return url != null ? url.ToString() : string.Empty;
        }
    }
}
