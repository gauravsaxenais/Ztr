using System.IO;
using Business.Configuration;
using ZTR.Framework.Business.Models;

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

    /// <summary>r
    /// Returns list of all the modules.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleManager" />
    public class ModuleManager : Manager, IModuleManager
    {
        private readonly IModuleServiceManager _moduleServiceManager;
        private readonly ILogger _logger;
        private readonly ModuleBlockGitConnectionOptions _moduleGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleServiceManager">The module service manager.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        public ModuleManager(ILogger<ModuleManager> logger, IModuleServiceManager moduleServiceManager, ModuleBlockGitConnectionOptions moduleGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleServiceManager, nameof(moduleServiceManager));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));

            _moduleServiceManager = moduleServiceManager;
            _logger = logger;
            _moduleGitConnectionOptions = moduleGitConnectionOptions;

            SetGitRepoConnection();
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

            var listOfModules = await _moduleServiceManager.GetAllModulesAsync(firmwareVersion, deviceType,
                _moduleGitConnectionOptions.ModulesConfig,
                _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceTomlFile,
                _moduleGitConnectionOptions.MetaToml).ConfigureAwait(false);

            return listOfModules;
        }

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        public void SetGitRepoConnection()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _moduleGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder);
            _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder = Path.Combine(_moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
            _moduleGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.ModulesConfig);

            _moduleServiceManager.SetGitRepoConnection(_moduleGitConnectionOptions);
        }
    }
}