namespace Business.RequestHandlers.Managers
{
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using Nett;
    using Parsers.ProtoParser.Parser;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
        private readonly IModuleServiceManager _moduleServiceManager;
        private readonly IProtoMessageParser _protoParser;
        private readonly ICustomMessageParser _customMessageParser;
        private readonly IModuleParser _moduleParser;

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
        }

        /// <summary>
        /// Gets the default values for all modules in asynchronous fashion.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ModuleReadModel>> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType)
        {
            // read default values from toml file defaults.toml
            var defaultValueFromTomlFile =
                await _moduleServiceManager.GetDefaultTomlFileContent(firmwareVersion, deviceType);

            // get list of all modules.
            var listOfModules = await _moduleServiceManager.GetAllModulesAsync(firmwareVersion, deviceType);

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
            var degreeOfParallelism = 10;

            await Task.WhenAll(
                from partition in Partitioner.Create(listOfModules).GetPartitions(degreeOfParallelism)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await MergeDefaultValuesWithModules(defaultValueFromTomlFile, partition.Current);
                }));
        }

        private async Task MergeDefaultValuesWithModules(string defaultValueFromTomlFile, ModuleReadModel module)
        {
            // get proto files for corresponding module and their uuid
            var protoFilePath = _moduleServiceManager.GetProtoFiles(module);

            if (!string.IsNullOrWhiteSpace(protoFilePath))
            {
                // get protoparsed messages from the proto files.
                var message = await _protoParser.GetCustomMessages(protoFilePath).ConfigureAwait(false);

                var formattedMessage = _customMessageParser.Format(message.Message);
                formattedMessage.Name = module.Name;

                var configValues = GetConfigValues(defaultValueFromTomlFile, module.Name);

                var jsonModels = _moduleParser.MergeTomlWithProtoMessage(configValues, formattedMessage);
                module.Config = jsonModels;
            }
        }

        private Dictionary<string, object> GetConfigValues(string fileContent, string moduleName)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();
            var fileData = Toml.ReadString(fileContent, tomlSettings);

            var dictionary = fileData.ToDictionary();
            var listOfModules = (Dictionary<string, object>[])dictionary["module"];

            // here message.name means Power, j1939 etc.
            var module = listOfModules.FirstOrDefault(dic => dic.Values.Contains<object>(moduleName));

            var configValues = new Dictionary<string, object>();

            if (module?.ContainsKey("config") == true)
            {
                configValues = (Dictionary<string, object>)module["config"];
            }

            return configValues;
        }
    }
}
