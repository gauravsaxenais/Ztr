namespace Business.RequestHandlers.Managers
{
    using Business.GitRepositoryWrappers.Interfaces;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
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
        private readonly ILogger _logger;
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
            _logger = logger;
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync(string deviceType)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(deviceType);

            _logger.LogInformation($"{Prefix}: methodName: {nameof(GetAllFirmwareVersionsAsync)} Getting list of compatible firmware versions based on a device type {deviceType}.");
            var listFirmwareVersions = await _firmwareVersionServiceManager.GetAllFirmwareVersionsAsync(deviceType)
                .ConfigureAwait(false);

            return listFirmwareVersions;
        }
    }
}
