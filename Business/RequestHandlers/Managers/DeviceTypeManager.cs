namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System;
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
        public async Task<ApiResponse> GetAllDevicesAsync()
        {
            var prefix = nameof(DeviceTypeManager);
            ApiResponse apiResponse = null;

            try
            {
                Logger.LogInformation($"{prefix}: Getting list of all devices.");

                var listOfDevices = await _moduleServiceManager.GetAllDevicesAsync();

                apiResponse = new ApiResponse(status: true, data: listOfDevices);
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, $"{prefix}: Error occured while getting list of all devices.");
                apiResponse = new ApiResponse(ErrorType.BusinessError, exception);
            }

            return apiResponse;
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse> GetAllFirmwareVersionsAsync()
        {
            var prefix = nameof(DeviceTypeManager);
            ApiResponse apiResponse = null;

            try
            {
                Logger.LogInformation($"{prefix}: Getting list of all firmware versions");

                var listFirmwareVersions = await _moduleServiceManager.GetAllFirmwareVersionsAsync();

                apiResponse = new ApiResponse(status: true, data: listFirmwareVersions);
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, $"{prefix}: Error occured while getting list of all firmware versions.");
                apiResponse = new ApiResponse(ErrorType.BusinessError, exception);
            }
            
            return apiResponse;
        }
    }
}
