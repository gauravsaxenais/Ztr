namespace Business.RequestHandlers.Managers
{
    using Business.GitRepositoryWrappers.Interfaces;
    using EnsureThat;
    using Interfaces;
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
        private readonly IDeviceServiceManager _deviceServiceManager;
        private const string Prefix = nameof(DeviceTypeManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTypeManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="deviceServiceManager">The module service manager.</param>
        public DeviceTypeManager(ILogger<DeviceTypeManager> logger, IDeviceServiceManager deviceServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(deviceServiceManager, nameof(deviceServiceManager));

            _deviceServiceManager = deviceServiceManager;
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse> GetAllDevicesAsync()
        {
            ApiResponse apiResponse;

            try
            {
                Logger.LogInformation($"{Prefix} method name: {nameof(GetAllDevicesAsync)}: Getting list of all devices.");

                var listOfDevices = await _deviceServiceManager.GetAllDevicesAsync();

                apiResponse = new ApiResponse(status: true, data: listOfDevices);
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, $"{Prefix}: method name: {nameof(GetAllDevicesAsync)} Error occurred while getting list of all devices.");
                apiResponse = new ApiResponse(ErrorType.BusinessError, exception);
            }

            return apiResponse;
        }
    }
}
