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

    /// <summary>
    /// This manager takes config.toml as input and returns
    /// list of modules and blocks.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IConfigCreateFromManager" />
    public class ConfigCreateFromManager : Manager, IConfigCreateFromManager
    {
        /// <summary>Initializes a new instance of the <see cref="ConfigCreateFromManager" /> class.</summary>
        /// <param name="logger">The logger.</param>
        public ConfigCreateFromManager(ILogger<ConfigCreateFromManager> logger) : base(logger) { }

        /// <summary>
        /// Returns the list of all modules and blocks from config.toml.
        /// </summary>
        /// <param name="configTomlFile">config.toml as string.</param>
        /// <returns></returns>
        public async Task<string> GenerateConfigTomlModelAsync(string configTomlFile)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(configTomlFile);

            var listOfModules = new List<ModuleReadModel>();

            var data = GetTomlData(configTomlFile);

            listOfModules = data.Module;

            listOfModules = listOfModules.Select((module, index) => new ModuleReadModel { Id = index, Config = module.Config, Name = module.Name, UUID = module.UUID }).ToList();

            //return listOfModules;
            return string.Empty;
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
    }
}
