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
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// Returns list of all the modules.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleManager" />
    public class ModuleManager : Manager, IModuleManager
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DeviceGitConnectionOptions _moduleGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        public ModuleManager(ILogger<ModuleManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions moduleGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));

            _gitRepoManager = gitRepoManager;
            _moduleGitConnectionOptions = moduleGitConnectionOptions;
        }

        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<ApiResponse> GetAllModulesAsync(string firmwareVersion, string deviceType)
        {
            var prefix = nameof(ModuleManager);
            ApiResponse apiResponse = null;

            var listOfModules = new List<ModuleReadModel>();

            try
            {
                Logger.LogInformation($"{prefix}: Getting list of modules for firmware version: {firmwareVersion} and device type: {deviceType}");

                SetGitRepoConnections();

                listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType);
                apiResponse = new ApiResponse(status: true, data: listOfModules);
            }
            catch(Exception exception)
            {
                Logger.LogCritical(exception, $"{prefix}: Error occured while getting list of modules for firmware version: {firmwareVersion} and device type: {deviceType}");
                apiResponse = new ApiResponse(ErrorType.BusinessError, exception);
            }

            return apiResponse;
        }

        private void SetGitRepoConnections()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _moduleGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder);
            _gitRepoManager.SetConnectionOptions(_moduleGitConnectionOptions);
        }

        private async Task<List<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType)
        {
            var listOfModules = new List<ModuleReadModel>();

            var fileContent = await GetDeviceDataFromFirmwareVersionAsync(firmwareVersion, deviceType);

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                var data = GetTomlData(fileContent);

                listOfModules = data.Module;
            }

            return listOfModules;
        }

        private ConfigurationReadModel GetTomlData(string fileContent)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();
            var tomlData = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(data: fileContent, settings: tomlSettings);

            return tomlData;
        }

        private async Task<string> GetDeviceDataFromFirmwareVersionAsync(string firmwareVersion, string deviceType)
        {
            var gitConnectionOptions = (DeviceGitConnectionOptions)_gitRepoManager.GetConnectionOptions();

            var listOfFiles = await _gitRepoManager.GetFileDataFromTagAsync(firmwareVersion, gitConnectionOptions.DefaultTomlConfiguration.DeviceTomlFile);

            // case insensitive search.
            var deviceTypeFile = listOfFiles.Where(p => p.FileName?.IndexOf(deviceType, StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();

            var fileContent = string.Empty;

            if (deviceTypeFile != null)
            {
                fileContent = System.Text.Encoding.UTF8.GetString(deviceTypeFile.Data);
            }

            return fileContent;
        }
    }
}