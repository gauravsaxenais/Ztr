namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.Models;
    using Business.Parsers.ProtoParser.Models;
    using Business.Parsers.ProtoParser.Parser;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// This class returns all the modules, their name and uuid information.
    /// It also returns of the default values for all the modules.
    /// It integrates with module parser to parse a proto file,
    /// recieves all the default values from default.toml and 
    /// receives the module.proto from corresponding module name
    /// and uuid folder.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDefaultValueManager" />
    public class DefaultValueManager : Manager, IDefaultValueManager
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;
        private readonly string protoFileName = "module.proto";
        private readonly IProtoMessageParser _protoParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValueManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        /// /// <param name="protoParser">File loader which reads a proto file as input and gives out custom message.</param>
        public DefaultValueManager(ILogger<DefaultValueManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions deviceGitConnectionOptions, IProtoMessageParser protoParser) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));
            EnsureArg.IsNotNull(deviceGitConnectionOptions.DefaultTomlConfiguration, nameof(deviceGitConnectionOptions.DefaultTomlConfiguration));

            _protoParser = protoParser;
            _gitRepoManager = gitRepoManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;
        }

        /// <summary>
        /// Gets the default values for all modules in asynchronous fashion.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ModuleReadModel>> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType)
        {
            SetConnection();

            // 1. get list of modules based on their firmware version and device type.
            // 2. get protofile paths based on firmware version and device type.
            // 3. create custom message for each of protofiles.
            // 4. get list of modules and their custom messages.
            var modulesProtoFolder = _deviceGitConnectionOptions.ModulesConfig;

            // read default values from toml file defaults.toml
            var defaultValueFromTomlFile = await GetDefaultValues(firmwareVersion, deviceType);

            // get list of all modules.
            var listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType);

            await MergeValuesWithModulesAsync(defaultValueFromTomlFile, listOfModules, modulesProtoFolder);

            return listOfModules;
        }

        /// <summary>
        /// Merges the values with modules asynchronous.
        /// </summary>
        /// <param name="defaultValueFromTomlFile">The default value from toml file.</param>
        /// <param name="listOfModules">The list of modules.</param>
        /// <param name="modulesProtoFolder">The modules proto folder.</param>
        public async Task MergeValuesWithModulesAsync(string defaultValueFromTomlFile, IEnumerable<ModuleReadModel> listOfModules, string modulesProtoFolder)
        {
            await Task.WhenAll(
                from partition in Partitioner.Create(listOfModules).GetPartitions(10)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await MergeDefaultValuesWithModules(defaultValueFromTomlFile, partition.Current, modulesProtoFolder);
                }));
        }

        private async Task MergeDefaultValuesWithModules(string defaultValueFromTomlFile, ModuleReadModel module, string modulesProtoFolder)
        {
            var customMessageParser = new CustomMessageParser();
            var moduleParser = new ModuleParser();

            // get proto files for corresponding module and their uuid
            var protoFilePaths = GetProtoFiles(modulesProtoFolder, module);

            // get protoparsed messages from the proto files.
            var message = await GetCustomMessages(protoFilePaths).ConfigureAwait(false);

            var formattedMessage = customMessageParser.Format(message.Message);
            formattedMessage.Name = module.Name;

            var jsonModels = moduleParser.GetJsonFromTomlAndProtoFile(defaultValueFromTomlFile, formattedMessage);
            module.Config = jsonModels;
        }

        private void SetConnection()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);
            _deviceGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder = Path.Combine(_deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
            _deviceGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.ModulesConfig);

            _gitRepoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }

        private async Task<CustomMessage> GetCustomMessages(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            string protoDirectory = new FileInfo(filePath).Directory.FullName;

            var result = await _protoParser.GetProtoParsedMessage(fileName, protoDirectory).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Gets the proto files.
        /// </summary>
        /// <param name="moduleFilePath">The module file path.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns></returns>
        private string GetProtoFiles(string moduleFilePath, ModuleReadModel moduleName)
        {
            EnsureArg.IsNotNullOrWhiteSpace(moduleFilePath);

            var moduleFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFilePath, moduleName.Name);

            if (!string.IsNullOrWhiteSpace(moduleFolder))
            {
                var uuidFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFolder, moduleName.UUID);

                foreach (string file in Directory.EnumerateFiles(uuidFolder, protoFileName))
                {
                    return file;
                }
            }

            return string.Empty;
        }

        private async Task<string> GetDefaultValues(string firmwareVersion, string deviceType)
        {
            var gitConnectionOptions = (DeviceGitConnectionOptions)_gitRepoManager.GetConnectionOptions();

            var listOfFiles = await _gitRepoManager.GetFileDataFromTagAsync(firmwareVersion, gitConnectionOptions.DefaultTomlConfiguration.DefaultTomlFile)
                                                   .ConfigureAwait(false);

            // case insensitive search.
            var deviceTypeFile = listOfFiles.Where(p => p.FileName?.IndexOf(deviceType, StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();

            var fileContent = string.Empty;

            if (deviceTypeFile != null)
            {
                fileContent = System.Text.Encoding.UTF8.GetString(deviceTypeFile.Data);
            }

            return fileContent;
        }

        /// <summary>
        /// Gets the list of modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        private async Task<IEnumerable<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType)
        {
            var listOfModules = new List<ModuleReadModel>();

            var deviceTomlFileContent = await GetDeviceDataFromFirmwareVersionAsync(firmwareVersion, deviceType);
            if (!string.IsNullOrWhiteSpace(deviceTomlFileContent))
            {
                var data = GetTomlData(deviceTomlFileContent);

                listOfModules = data.Module;
            }

            // fix the indexes.
            listOfModules = listOfModules.Select((module, index) => new ModuleReadModel { Id = index, Config = module.Config, Name = module.Name, UUID = module.UUID }).ToList();

            return listOfModules;
        }

        /// <summary>
        /// Gets the toml data.
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <returns></returns>
        private ConfigurationReadModel GetTomlData(string fileContent)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            var tomlData = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(data: fileContent, settings: tomlSettings);

            return tomlData;
        }

        /// <summary>
        /// Gets the device data from firmware version asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        private async Task<string> GetDeviceDataFromFirmwareVersionAsync(string firmwareVersion, string deviceType)
        {
            var gitConnectionOptions = (DeviceGitConnectionOptions)_gitRepoManager.GetConnectionOptions();

            var listOfFiles = await _gitRepoManager.GetFileDataFromTagAsync(firmwareVersion, gitConnectionOptions.DefaultTomlConfiguration.DeviceTomlFile)
                                                   .ConfigureAwait(false);

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
