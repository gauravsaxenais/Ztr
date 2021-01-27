namespace Business.RequestHandlers.Managers
{
    using Business.Common.Models;
    using Business.GitRepository.Interfaces;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Returns list of all the modules.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleManager" />
    public class ModuleManager : Manager, IModuleManager
    {
        private readonly IModuleServiceManager _moduleServiceManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleServiceManager">The module service manager.</param>
        public ModuleManager(ILogger<ModuleManager> logger, IModuleServiceManager moduleServiceManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleServiceManager, nameof(moduleServiceManager));
            
            _moduleServiceManager = moduleServiceManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ModuleReadModel>> GetAllModulesAsync(string firmwareVersion, string deviceType)
        {
            var prefix = nameof(ModuleManager);

            _logger.LogInformation(
                $"{prefix}: methodName: {nameof(GetAllModulesAsync)}. Getting list of modules for firmware version: {firmwareVersion} and device type: {deviceType}");

            var listOfModules = await _moduleServiceManager.GetAllModulesAsync(firmwareVersion, deviceType).ConfigureAwait(false);

            return listOfModules;
        }
    }
}