namespace Business.RequestHandlers.Managers
{
    using Business.Models.ConfigModels;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Nett;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    public class DefaultValueManager : Manager, IDefaultValueManager
    {
        private readonly IGitRepositoryManager _repoManager;
        public DefaultValueManager(ILogger<ModuleManager> logger, IGitRepositoryManager repoManager, IEnvironmentSettings environmentSettings) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(environmentSettings, nameof(environmentSettings));

            _repoManager = repoManager;

            var environmentOptions = environmentSettings.GetDeviceGitConnectionOptions();
            _repoManager.SetConnectionOptions(environmentOptions);
        }

        public async Task<IEnumerable<ModuleReadModel>> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType)
        {
            var listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType);

            return listOfModules;
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

        public static List<T> ReadDataModel<T>(string data, string fieldToRead, TomlSettings settings) where T : class, new()
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(data, nameof(data));
            EnsureArg.IsNotNull(settings, nameof(settings));
            EnsureArg.IsNotEmptyOrWhiteSpace(fieldToRead, nameof(fieldToRead));

            TomlTable fileData = null;

            fileData = Toml.ReadString(data, settings);

            var readModels = (TomlTable)fileData[fieldToRead];

            var items = new List<T>();
            var dictionary = readModels.Rows.ToDictionary(t => t.Key, t => (object)t.Value.ToString());

            items.Add(DictionaryExtensions.ToObject<T>(dictionary));
            return items;
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

        private void GenerateCSharpFileFromProtoFile(string protoFileLocation, string csharpFileDirectory, string protoFileName)
        {
            // Use ProcessStartInfo class
            var psi = new ProcessStartInfo
            {
                FileName = @"C:\Users\gaurav.saxena\source\repos\ProtoAppGoogleProtoBuff\ProtoCompiler\protoc-3.13.0-win64\bin\protoc.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $" --include_imports --include_source_info --proto_path={protoFileLocation} --csharp_out={csharpFileDirectory} {protoFileName}"
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (var exeProcess = Process.Start(psi))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
