namespace Business.RequestHandlers.Managers
{
    using Business.Common.Models;
    using Business.GitRepository.Interfaces;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

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
            var listOfTags = await _firmwareVersionServiceManager.GetTagsEarlierThanThisTagAsync(module.FirmwareVersion).ConfigureAwait(false);
            
            foreach (var tag in listOfTags)
            {
                var moduleList = await _firmwareVersionServiceManager.GetListOfModulesAsync(tag, module.DeviceType).ConfigureAwait(false);

                var contained = module.Modules.Intersect(moduleList, new ModuleReadModelComparer()).Count() == module.Modules.Count();

                if (contained)
                {
                    firmwareVersions.Add(tag);
                }

                else break;
            }

            return firmwareVersions;
        }
    }
}
