namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using EnsureThat;
    using ZTR.Framework.Business.File.FileReaders;
    using Business.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using Business.Parsers;
    using Business.Configuration;
    using Business.Parsers.Models;
    using System;
    using Microsoft.AspNetCore.Http;
    using System.Text;

    /// <summary>
    /// This manager takes config.toml as input and returns
    /// list of modules and blocks.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IConfigCreateFromManager" />
    public class ConfigCreateFromManager : Manager, IConfigCreateFromManager
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;
        private readonly string protoFileName = "module.proto";
        private readonly InputFileLoader _inputFileLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigCreateFromManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        /// <param name="inputFileLoader">The input file loader.</param>
        public ConfigCreateFromManager(ILogger<DefaultValueManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions deviceGitConnectionOptions, InputFileLoader inputFileLoader) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));
            EnsureArg.IsNotNull(deviceGitConnectionOptions.DefaultTomlConfiguration, nameof(deviceGitConnectionOptions.DefaultTomlConfiguration));

            _inputFileLoader = inputFileLoader;
            _gitRepoManager = gitRepoManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;

            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);
            _deviceGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.ModulesConfig);

            _gitRepoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }

        /// <summary>
        /// Returns the list of all modules and blocks from config.toml.
        /// </summary>
        /// <param name="configTomlFile">config.toml as string.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ModuleReadModel>> GenerateConfigTomlModelAsync(IFormFile configTomlFile)
        {
            EnsureArg.IsNotNull(configTomlFile);

            var configTomlFileContent = await ReadAsStringAsync(configTomlFile);

            var modules = await GetValuesFromTomlFileAsync(configTomlFileContent);

            return modules;
        }

        private async Task<string> ReadAsStringAsync(
                IFormFile file)
        {
            var builder = new StringBuilder();

            using var reader = new StreamReader(file.OpenReadStream());
            while (reader.Peek() >= 0)
            {
                builder.AppendLine(await reader.ReadLineAsync());
            }
            return builder.ToString();
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

                        foreach (string file in Directory.EnumerateFiles(uuidFolder, ""))
                        {
                            protoFilePath.Add(moduleName.Name, file);
                        }
                    }
                }
            }

            return protoFilePath;
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
        /// Gets the default values all modules asynchronous.
        /// </summary>
        /// <param name="configTomlFile">The configuration toml file.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ModuleReadModel>> GetValuesFromTomlFileAsync(string configTomlFile)
        {
            // 1. get list of modules based on their firmware version and device type.
            // 2. get protofile paths based on firmware version and device type.
            // 3. create custom message for each of protofiles.
            // 4. get list of modules and their custom messages.

            var customMessageParser = new CustomMessageParser();
            var moduleParser = new ModuleParser();

            var modulesProtoFolder = _deviceGitConnectionOptions.ModulesConfig;
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            // get list of all modules.
            var listOfModules = GetListOfModules(configTomlFile);

            // get proto files for corresponding module and their uuid
            var protoFilePaths = GetProtoFiles(modulesProtoFolder, listOfModules);

            // get protoparsed messages from the proto files.
            var messages = await GetCustomMessages(protoFilePaths).ConfigureAwait(false);

            for (int temp = 0; temp < messages.Count; temp++)
            {
                var formattedMessage = customMessageParser.Format(messages[temp].Message);
                formattedMessage.Name = messages[temp].Name;

                var jsonModels = new List<JsonModel>();

                jsonModels = moduleParser.GetJsonFromTomlAndProtoFile(configTomlFile, tomlSettings, formattedMessage);

                var module = listOfModules.Where(p => p.Name?.IndexOf(formattedMessage.Name, StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();

                if (module != null)
                {
                    module.Config = jsonModels;
                }
            }

            return listOfModules;
        }

        /// <summary>
        /// Gets the list of modules.
        /// </summary>
        /// <param name="configTomlFile">The configuration toml file.</param>
        /// <returns></returns>
        private IEnumerable<ModuleReadModel> GetListOfModules(string configTomlFile)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(configTomlFile);

            var listOfModules = new List<ModuleReadModel>();

            var data = GetTomlData(configTomlFile);

            listOfModules = data.Module;

            listOfModules = listOfModules.Select((module, index) => new ModuleReadModel { Id = index, Config = module.Config, Name = module.Name, UUID = module.UUID }).ToList();

            return listOfModules;
        }

        /// <summary>
        /// Gets the custom messages.
        /// </summary>
        /// <param name="protoFilePaths">The proto file paths.</param>
        /// <returns></returns>
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
    }
}
