namespace Business.RequestHandlers.Managers
{
    using Models;
    using Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System;
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
        private readonly IModuleServiceManager _moduleServiceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibleFirmwareVersionManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleServiceManager">The module service manager.</param>
        public CompatibleFirmwareVersionManager(ILogger<DefaultValueManager> logger, IModuleServiceManager moduleServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleServiceManager, nameof(moduleServiceManager));

            _moduleServiceManager = moduleServiceManager;
        }

        /// <summary>
        /// Gets the compatible firmware versions asynchronous.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        public async Task<ApiResponse> GetCompatibleFirmwareVersionsAsync(CompatibleFirmwareVersionReadModel module)
        {
            EnsureArg.IsNotNull(module);
            EnsureArg.IsNotEmptyOrWhiteSpace(module.FirmwareVersion);
            EnsureArg.IsNotEmptyOrWhiteSpace(module.DeviceType);
            EnsureArg.HasItems(module.Modules);

            var prefix = nameof(CompatibleFirmwareVersionManager);
            ApiResponse apiResponse = null;
            var firmwareVersions = new List<string>();

            try
            {
                Logger.LogInformation($"{prefix}: Getting list of compatible firmware versions based on a firmware version.");
                
                var listOfTags = await _moduleServiceManager.GetTagsEarlierThanThisTagAsync(module.FirmwareVersion);

                foreach(var tag in listOfTags)
                {
                    var moduleList = await _moduleServiceManager.GetAllModulesAsync(tag, module.DeviceType).ConfigureAwait(false);

                    var contained = module.Modules.Intersect(moduleList, new ModuleReadModelComparer()).Count() == module.Modules.Count();

                    if (contained)
                    {
                        firmwareVersions.Add(tag);
                    }

                    else break;
                }
                
                apiResponse = new ApiResponse(status: true, data: firmwareVersions);
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, $"{prefix}: Error occurred while getting list of compatible firmware versions based on a firmware version.");
                apiResponse = new ApiResponse(false, exception.Message, ErrorType.BusinessError, exception);
            }

            return apiResponse;
        }
    }
}
