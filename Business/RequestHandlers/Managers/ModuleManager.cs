namespace Business.RequestHandlers.Managers
{
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    public class ModuleManager : Manager, IModuleManager
    {
        private readonly IGitRepositoryManager _repoManager;
        
        public ModuleManager(ILogger<ModuleManager> logger, IGitRepositoryManager repoManager, IEnvironmentSettings environmentSettings) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(environmentSettings, nameof(environmentSettings));

            _repoManager = repoManager;

            var environmentOptions = environmentSettings.GetDeviceGitConnectionOptions();
            _repoManager.SetConnectionOptions(environmentOptions);
        }

        public async Task<IEnumerable<ModuleReadModel>> GetAllModulesAsync(string firmwareVersion, string deviceType)
        {
            var listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType);

            return listOfModules;
        }

        public async Task<IEnumerable<ModuleReadModel>> GetModelByNameAsync(string firmwareVersion, string deviceType, IEnumerable<string> names)
        {
            var listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType);

            if (listOfModules != null && listOfModules.Any())
            {
                listOfModules = listOfModules.Where(x => names.Contains(x.Name)).ToList();
            }

            return listOfModules;
        }

        public async Task<IEnumerable<ModuleReadModel>> GetModuleByNameAsync(string name, string firmwareVersion, string deviceType,  params string[] names)
        {
            EnsureArg.IsNotNull(name, nameof(name));
            return await GetModelByNameAsync(firmwareVersion, deviceType, names.Prepend(name)).ConfigureAwait(false);
        }

        private async Task<IEnumerable<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType)
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
            var gitConnectionOptions = _repoManager.GetConnectionOptions();

            var listOfFiles = await _repoManager.GetFileDataFromTagAsync(firmwareVersion, gitConnectionOptions.TomlConfiguration.DeviceTomlFile);

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
