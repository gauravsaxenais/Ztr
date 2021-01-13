namespace Business.RequestHandlers.Managers
{
    using Business.GitRepositoryWrappers.Interfaces;
    using Business.Parsers.ProtoParser.Models;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using Nett;
    using Parsers.ProtoParser.Parser;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// This class returns all the modules, their name and uuid information.
    /// It also returns of the default values for all the modules.
    /// It integrates with module parser to parse a proto file,
    /// receives all the default values from default.toml and 
    /// receives the module.proto from corresponding module name
    /// and uuid folder.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDefaultValueManager" />
    public class DefaultValueManager : Manager, IDefaultValueManager
    {
        private readonly IModuleServiceManager _moduleServiceManager;
        private readonly IProtoMessageParser _protoParser;
        private readonly ICustomMessageParser _customMessageParser;
        private readonly IModuleParser _moduleParser;
        private readonly ILogger _logger;
        private const string Prefix = nameof(DefaultValueManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValueManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleServiceManager">The module service manager.</param>
        /// <param name="protoParser">The proto parser.</param>
        /// <param name="customMessageParser">The custom message parser.</param>
        /// <param name="moduleParser">The module parser.</param>
        public DefaultValueManager(ILogger<DefaultValueManager> logger,
                                    IModuleServiceManager moduleServiceManager,
                                    IProtoMessageParser protoParser,
                                    ICustomMessageParser customMessageParser,
                                    IModuleParser moduleParser) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleServiceManager, nameof(moduleServiceManager));
            EnsureArg.IsNotNull(protoParser, nameof(protoParser));
            EnsureArg.IsNotNull(customMessageParser, nameof(customMessageParser));
            EnsureArg.IsNotNull(moduleParser, nameof(moduleParser));

            _moduleServiceManager = moduleServiceManager;
            _protoParser = protoParser;
            _customMessageParser = customMessageParser;
            _moduleParser = moduleParser;
            _logger = logger;
        }

        /// <summary>
        /// Gets the default values for all modules in asynchronous fashion.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ModuleReadModel>> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType)
        {
            _logger.LogInformation($"{Prefix}: Getting default values for {firmwareVersion} and {deviceType}.");

            // read default values from toml file defaults.toml
            var defaultValueFromTomlFile =
                await _moduleServiceManager.GetDefaultTomlFileContentAsync(firmwareVersion, deviceType).ConfigureAwait(false);

            _logger.LogInformation($"{Prefix}: Getting list of modules {firmwareVersion} and {deviceType}.");

            // get list of all modules.
            var listOfModules = await _moduleServiceManager.GetAllModulesAsync(firmwareVersion, deviceType)
                .ConfigureAwait(false);

            _logger.LogInformation($"{Prefix}: Merging default values with module information. {firmwareVersion} and {deviceType}.");
            await MergeValuesWithModulesAsync(defaultValueFromTomlFile, listOfModules);

            return listOfModules;
        }

        /// <summary>
        /// Merges the values with modules asynchronous.
        /// </summary>
        /// <param name="defaultValueFromTomlFile">The default value from toml file.</param>
        /// <param name="listOfModules">The list of modules.</param>
        public async Task MergeValuesWithModulesAsync(string defaultValueFromTomlFile, IEnumerable<ModuleReadModel> listOfModules)
        {
            _logger.LogInformation($"{Prefix}: method name: {nameof(MergeValuesWithModulesAsync)} Getting proto file for modules.");

            var moduleReadModels = listOfModules.ToList();
            var protoFilePaths = GetProtoFilePaths(moduleReadModels);

            var customMessages = await GetCustomMessages(protoFilePaths).ConfigureAwait(false);
            _logger.LogInformation($"{Prefix}: method name: {nameof(MergeValuesWithModulesAsync)} Retrieved proto files for modules.");

            foreach (var module in moduleReadModels)
            {
                var customMessage = customMessages.FirstOrDefault(x => x.Name == module.Name);
                module.Config = MergeDefaultValuesWithModuleAsync(defaultValueFromTomlFile, module.Name, customMessage);

                if (customMessage != null)
                {
                    customMessage.Message = null;
                    customMessage = null;
                }
            }
        }

        private async Task<List<CustomMessage>> GetCustomMessages(Dictionary<string, string> protoFilePaths)
        {
            var customMessages = new List<CustomMessage>();

            foreach (var filePath in protoFilePaths)
            {
                var moduleName = filePath.Key;

                var customMessage =  await _protoParser.GetCustomMessage(filePath.Value, moduleName).ConfigureAwait(false);
                customMessages.Add(customMessage);
            }
            
            return customMessages;
        }

        private Dictionary<string, string> GetProtoFilePaths(IEnumerable<ModuleReadModel> listOfModules)
        {
            return listOfModules.ToDictionary(module => module.Name, module => _moduleServiceManager.GetProtoFiles(module));
        }

        private IEnumerable<JsonField> MergeDefaultValuesWithModuleAsync(string defaultValueFromTomlFile, string moduleName, CustomMessage message)
        {
            var formattedMessage = _customMessageParser.Format(message.Message);
            formattedMessage.Name = moduleName;

            _logger.LogInformation($"{Prefix}: method name: {nameof(MergeDefaultValuesWithModuleAsync)} Getting config values from default.toml file for {moduleName}");
            var configValues = GetConfigValues(defaultValueFromTomlFile, moduleName);

            _logger.LogInformation($"{Prefix}: method name: {nameof(MergeDefaultValuesWithModuleAsync)} Merging config values with protoparsed message for {moduleName}");
            var jsonModels = _moduleParser.MergeTomlWithProtoMessage(configValues, formattedMessage);

            formattedMessage.ClearData(formattedMessage);
            formattedMessage = null;

            return jsonModels;
        }

        private Dictionary<string, object> GetConfigValues(string fileContent, string moduleName)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();
            var fileData = Toml.ReadString(fileContent, tomlSettings);

            var configValues = new Dictionary<string, object>();

            var dictionary = fileData.ToDictionary();
            var listOfModules = (Dictionary<string, object>[])dictionary["module"];

            // here message.name means Power, j1939 etc.
            var module = listOfModules.FirstOrDefault(dic => dic.Values.Contains<object>(moduleName));

            if (module?.ContainsKey("config") == true)
            {
                configValues = (Dictionary<string, object>)module["config"];
            }

            return configValues;
        }
    }
}
