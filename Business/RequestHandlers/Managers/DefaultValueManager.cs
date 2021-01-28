namespace Business.RequestHandlers.Managers
{
    using Business.Common.Models;
    using Business.GitRepository.Interfaces;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
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

            // Clone repository here.
            await _moduleServiceManager.CloneGitHubRepoAsync().ConfigureAwait(false);

            // read default values from toml file defaults.toml
            var defaultValueFromTomlFile =
                await _moduleServiceManager.GetDefaultTomlFileContentAsync(firmwareVersion, deviceType).ConfigureAwait(false);

            _logger.LogInformation($"{Prefix}: Getting list of modules {firmwareVersion} and {deviceType}.");

            // get list of all modules.
            var listOfModules = await _moduleServiceManager.GetAllModulesAsync(firmwareVersion, deviceType).ConfigureAwait(false);

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
            _logger.LogInformation($"{Prefix}: method name: {nameof(MergeValuesWithModulesAsync)} Merging default values with list of modules in parallel...");

            var moduleReadModels = listOfModules.ToList();

            var modulesTasks = new List<Task>();

            for (var index = 0; index < moduleReadModels.Count(); index++)
            {
                modulesTasks.Add(MergeDefaultValuesWithModuleAsync(defaultValueFromTomlFile, moduleReadModels.ElementAt(index)));
            }

            await Task.WhenAll(modulesTasks);
        }

        private async Task MergeDefaultValuesWithModuleAsync(string defaultValueFromTomlFile, ModuleReadModel module)
        {
            EnsureArg.IsNotNull(module);

            _logger.LogInformation($"{Prefix}: method name: {nameof(MergeDefaultValuesWithModuleAsync)} Getting proto file for {module.Name}");

            // get proto files for corresponding module and their uuid
            var protoFilePath = _moduleServiceManager.GetProtoFiles(module);

            _logger.LogInformation($"{Prefix}: method name: {nameof(MergeDefaultValuesWithModuleAsync)} Retrieved proto file for {module.Name}");
            if (!string.IsNullOrWhiteSpace(protoFilePath))
            {
                // get proto parsed messages from the proto files.
                var message = await _protoParser.GetCustomMessage(protoFilePath).ConfigureAwait(false);

                var formattedMessage = _customMessageParser.Format(message.Message);
                formattedMessage.Name = module.Name;

                _logger.LogInformation($"{Prefix}: method name: {nameof(MergeDefaultValuesWithModuleAsync)} Getting config values from default.toml file for {module.Name}");
                var configValues = GetConfigValues(defaultValueFromTomlFile, module.Name);

                _logger.LogInformation($"{Prefix}: method name: {nameof(MergeDefaultValuesWithModuleAsync)} Merging config values with protoparsed message for {module.Name}");
                var jsonModels = _moduleParser.MergeTomlWithProtoMessage(configValues, formattedMessage);
                module.Config = jsonModels;

                message.Message = null;
                message = null;

                formattedMessage.ClearData(formattedMessage);
                formattedMessage = null;
            }
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
