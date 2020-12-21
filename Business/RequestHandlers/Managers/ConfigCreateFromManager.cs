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
        public async Task<List<ModuleReadModel>> GenerateConfigTomlModelAsync(string configTomlFile)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(configTomlFile);

            var listOfModules = new List<ModuleReadModel>();

            var data = GetTomlData(configTomlFile);

            listOfModules = data.Module;

            listOfModules = listOfModules.Select((module, index) => new ModuleReadModel { Id = index, Config = module.Config, Name = module.Name, UUID = module.UUID }).ToList();

            return listOfModules;
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
    }
}
