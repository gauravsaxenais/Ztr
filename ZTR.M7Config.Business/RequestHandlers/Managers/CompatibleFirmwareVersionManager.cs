namespace ZTR.M7Config.Business.RequestHandlers.Managers
{
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.M7Config.Business.GitRepository.Interfaces;

    /// <summary>
    /// This manager takes input of firmware version
    /// and returns list of firmware versions as an array.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="ICompatibleFirmwareVersionManager" />
    public class CompatibleFirmwareVersionManager : Manager, ICompatibleFirmwareVersionManager
    {
        private readonly IFirmwareVersionServiceManager _firmwareVersionServiceManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibleFirmwareVersionManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="firmwareVersionServiceManager">The firmware version service manager.</param>
        public CompatibleFirmwareVersionManager(ILogger<DefaultValueManager> logger, IFirmwareVersionServiceManager firmwareVersionServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(firmwareVersionServiceManager, nameof(firmwareVersionServiceManager));

            _firmwareVersionServiceManager = firmwareVersionServiceManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets the compatible firmware versions asynchronous.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetCompatibleFirmwareVersionsAsync(CompatibleFirmwareVersionReadModel module)
        {
            EnsureArg.IsNotNull(module);
            EnsureArg.IsNotEmptyOrWhiteSpace(module.FirmwareVersion);
            EnsureArg.IsNotEmptyOrWhiteSpace(module.DeviceType);
            EnsureArg.HasItems(module.Modules);

            var prefix = nameof(CompatibleFirmwareVersionManager);
            var firmwareVersions = new List<string>();

            _logger.LogInformation($"{prefix}: methodName: {nameof(GetCompatibleFirmwareVersionsAsync)} Getting list of compatible firmware versions based on a firmware version.");
            _logger.LogInformation($"{prefix}: methodName: {nameof(GetCompatibleFirmwareVersionsAsync)} Cloning firmware version git repository.");
            var firmwareVersionUrl = await _firmwareVersionServiceManager.GetFirmwareUrlAsync(module.DeviceType).ConfigureAwait(false);
            _firmwareVersionServiceManager.SetGitRepoUrl(module.DeviceType, firmwareVersionUrl);
            await _firmwareVersionServiceManager.CloneGitRepoAsync().ConfigureAwait(false);
            var listOfTags = await _firmwareVersionServiceManager.GetAllFirmwareVersionsAsync().ConfigureAwait(false);
            listOfTags = listOfTags.Where(x => !string.Equals(x, module.FirmwareVersion, StringComparison.OrdinalIgnoreCase)).ToList();
            firmwareVersions = await _firmwareVersionServiceManager.GetCompatibleFirmwareVersions(listOfTags, module.FirmwareVersion, module.DeviceType, module.Modules);

            return firmwareVersions;
        }
    }
}
