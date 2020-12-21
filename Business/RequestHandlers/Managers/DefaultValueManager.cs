namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.Models;
    using Business.Parsers;
    using Business.Parsers.Models;
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
        private readonly InputFileLoader _inputFileLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValueManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        /// /// <param name="inputFileLoader">File loader which reads a proto file as input and gives out custom message.</param>
        public DefaultValueManager(ILogger<DefaultValueManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions deviceGitConnectionOptions, InputFileLoader inputFileLoader) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));
            EnsureArg.IsNotNull(deviceGitConnectionOptions.DefaultTomlConfiguration, nameof(deviceGitConnectionOptions.DefaultTomlConfiguration));

            _inputFileLoader = inputFileLoader;
            _gitRepoManager = gitRepoManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;

            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);
            _deviceGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder = Path.Combine(_deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
            _deviceGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.ModulesConfig);

            _gitRepoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }

        /// <summary>
        /// Gets the default values for all modules in asynchronous fashion.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ModuleReadModel>> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType)
        {
            // 1. get list of modules based on their firmware version and device type.
            // 2. get protofile paths based on firmware version and device type.
            // 3. create custom message for each of protofiles.
            // 4. get list of modules and their custom messages.

            var customMessageParser = new CustomMessageParser();
            var moduleParser = new ModuleParser();
          
            var modulesProtoFolder = Path.Combine(_deviceGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder, deviceType, _deviceGitConnectionOptions.DefaultTomlConfiguration.ModulesProtoFolder);
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            // get list of all modules.
            var listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType);

            // read default values from toml file defaults.toml
            var defaultValueFromTomlFile = await GetDefaultValues(firmwareVersion, deviceType);

            // get proto files for corresponding module and their uuid
            var protoFilePaths = GetProtoFiles(modulesProtoFolder, listOfModules);

            // get protoparsed messages from the proto files.
            var messages = await GetCustomMessages(protoFilePaths).ConfigureAwait(false);

            for (int temp = 0; temp < messages.Count; temp++)
            {
                var formattedMessage = customMessageParser.Format(messages[temp].Message);
                formattedMessage.Name = messages[temp].Name;

                var jsonModels = new List<JsonModel>();

                jsonModels = moduleParser.GetJsonFromTomlAndProtoFile(defaultValueFromTomlFile, tomlSettings, formattedMessage);
                
                var module = listOfModules.Where(p => p.Name?.IndexOf(formattedMessage.Name, StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();

                if (module != null)
                {
                    module.Config = jsonModels;
                }
            }

            return listOfModules;
        }

        private async Task<List<CustomMessage>> GetCustomMessages(Dictionary<string, string> protoFilePaths)
        {
            var tasks = new List<Task<CustomMessage>>();
            var result = new List<CustomMessage>();
                       
            foreach (var filePath in protoFilePaths)
            {
                var fileName = Path.GetFileName(filePath.Value);
                var moduleName = filePath.Key;

                string protoDirectory = new FileInfo(filePath.Value).Directory.FullName;

                tasks.Add(_inputFileLoader.GenerateCodeFiles(moduleName, fileName, protoDirectory));
            }
            
            Task.WaitAll(tasks.ToArray());

            foreach (var taskResult in tasks)
            {
                if (taskResult != null)
                {
                    result.Add(taskResult.Result);
                }
            }
          
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Gets the proto file path.
        /// </summary>
        /// <param name="moduleFilePath">The module file path.</param>
        /// <param name="listOfModules">The list of modules.</param>
        /// <returns></returns>
        private Dictionary<string, string> GetProtoFiles(string moduleFilePath, IEnumerable<ModuleReadModel> listOfModules)
        {
            EnsureArg.IsNotNullOrWhiteSpace(moduleFilePath);
            EnsureArg.IsNotNull(listOfModules);

            var protoFilePath = new Dictionary<string, string>();

            if (listOfModules.Any())
            {
                foreach (var moduleName in listOfModules)
                {
                    var moduleFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFilePath, moduleName.Name);

                    if (!string.IsNullOrWhiteSpace(moduleFolder))
                    {
                        var uuidFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFolder, moduleName.UUID);

                        foreach (string file in Directory.EnumerateFiles(uuidFolder, protoFileName))
                        {
                            protoFilePath.Add(moduleName.Name, file);
                        }
                    }
                }
            }

            return protoFilePath;
        }

        private async Task<string> GetDefaultValues(string firmwareVersion, string deviceType)
        {
            var gitConnectionOptions = (DeviceGitConnectionOptions)_gitRepoManager.GetConnectionOptions();

            var listOfFiles = await _gitRepoManager.GetFileDataFromTagAsync(firmwareVersion, gitConnectionOptions.DefaultTomlConfiguration.DefaultTomlFile);

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

            listOfModules = listOfModules.Select((module, index) => new ModuleReadModel  { Id = index, Config = module.Config, Name = module.Name, UUID = module.UUID }).ToList();

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
