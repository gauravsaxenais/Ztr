namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Returns device information.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDeviceTypeManager" />
    public class DeviceTypeManager : Manager, IDeviceTypeManager
    {
        private readonly IModuleServiceManager _moduleServiceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTypeManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleServiceManager">The module service manager.</param>
        public DeviceTypeManager(ILogger<DeviceTypeManager> logger, IModuleServiceManager moduleServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleServiceManager, nameof(moduleServiceManager));

            _moduleServiceManager = moduleServiceManager;
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            var listOfDevices = await _moduleServiceManager.GetAllDevicesAsync();
            
            return listOfDevices;
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync()
        {
            var listFirmwareVersions = await _moduleServiceManager.GetAllFirmwareVersionsAsync();

            return listFirmwareVersions;
        }
    }
}
