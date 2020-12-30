namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly IModuleManager _moduleManager;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibleFirmwareVersionManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="moduleManager">The module manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        public CompatibleFirmwareVersionManager(ILogger<DefaultValueManager> logger, IGitRepositoryManager gitRepoManager, IModuleManager moduleManager, DeviceGitConnectionOptions deviceGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(moduleManager, nameof(moduleManager));

            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));
            EnsureArg.IsNotNull(deviceGitConnectionOptions.DefaultTomlConfiguration, nameof(deviceGitConnectionOptions.DefaultTomlConfiguration));

            _gitRepoManager = gitRepoManager;
            _moduleManager = moduleManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;
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
                SetGitRepoConnections();

                await _gitRepoManager.CloneRepositoryAsync().ConfigureAwait(false);

                var listOfTags = await _gitRepoManager.GetTagsEarlierThanThisTagAsync(module.FirmwareVersion);

                foreach(var tag in listOfTags)
                {
                    var moduleList = await _moduleManager.GetListOfModulesAsync(tag, module.DeviceType).ConfigureAwait(false);

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
                Logger.LogCritical(exception, $"{prefix}: Error occured while getting list of compatible firmware versions based on a firmware version.");
                apiResponse = new ApiResponse(false, exception.Message, ErrorType.BusinessError, exception);
            }

            return apiResponse;
        }

        private void SetGitRepoConnections()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);
            _deviceGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.ModulesConfig);

            _gitRepoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }
    }
}
