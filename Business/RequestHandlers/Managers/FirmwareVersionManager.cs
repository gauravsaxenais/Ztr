namespace Business.RequestHandlers.Managers
{
    using Business.GitRepositoryWrappers.Interfaces;
    using Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Returns firmware version information.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IFirmwareVersionManager" />
    public class FirmwareVersionManager : Manager, IFirmwareVersionManager
    {
        private readonly IFirmwareVersionServiceManager _firmwareVersionServiceManager;
        private const string Prefix = nameof(FirmwareVersionManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="firmwareVersionServiceManager">The firmware version manager.</param>
        public FirmwareVersionManager(ILogger<DeviceTypeManager> logger, IFirmwareVersionServiceManager firmwareVersionServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(firmwareVersionServiceManager, nameof(firmwareVersionServiceManager));

            _firmwareVersionServiceManager = firmwareVersionServiceManager;
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse> GetAllFirmwareVersionsAsync(string deviceType)
        {
            EnsureArg.IsNotNull(deviceType);
            ApiResponse apiResponse;

            try
            {
                Logger.LogInformation($"{Prefix}: Getting list of all firmware versions");

                var listFirmwareVersions = await _firmwareVersionServiceManager.GetAllFirmwareVersionsAsync(deviceType)
                                                                                              .ConfigureAwait(false);

                apiResponse = new ApiResponse(status: true, data: listFirmwareVersions);
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, $"{Prefix}: Error occurred while getting list of all firmware versions.");
                apiResponse = new ApiResponse(ErrorType.BusinessError, exception);
            }
            
            return apiResponse;
        }
    }
}
