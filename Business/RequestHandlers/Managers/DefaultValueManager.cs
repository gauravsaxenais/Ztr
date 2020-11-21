namespace Business.RequestHandlers.Managers
{
    using Business.Models.ConfigModels;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Nett;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    public class DefaultValueManager : Manager, IDefaultValueManager
    {
        public DefaultValueManager(DeviceGitConnectionOptions gitConnectionOptions, ILogger<DefaultValueManager> logger) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitConnectionOptions, nameof(gitConnectionOptions));
            EnsureArg.IsNotNull(gitConnectionOptions.TomlConfiguration, nameof(gitConnectionOptions.TomlConfiguration));

            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            gitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, gitConnectionOptions.GitLocalFolder);
            gitConnectionOptions.TomlConfiguration.DeviceFolder = Path.Combine(gitConnectionOptions.GitLocalFolder, gitConnectionOptions.TomlConfiguration.DeviceFolder);
        }

        public async Task<IEnumerable<ModuleReadModel>> GetAllModulesAsync()
        {
            var moduleInformation = await GetModuleDataAsync();

            return moduleInformation;
        }

        public async Task<IEnumerable<string>> GetAllUuidsAsync()
        {
            var listOfModules = await GetModuleDataAsync();
            var uuids = new List<string>();

            if (listOfModules != null && listOfModules.Any())
            {
                uuids = listOfModules.Select(item => item.UUID).ToList();
            }

            return uuids;
        }

        public async Task<IEnumerable<ModuleReadModel>> GetModuleByNameAsync(IEnumerable<string> names)
        {
            var listOfModules = await GetModuleDataAsync();
            var uuids = new List<string>();

            listOfModules = listOfModules.Where(x => names.Contains(x.Name)).ToList();
            return listOfModules;
        }

        public async Task<IEnumerable<ModuleReadModel>> GetModuleByNameAsync(string name, params string[] names)
        {
            EnsureArg.IsNotNull(name, nameof(name));
            return await GetModuleByNameAsync(names.Prepend(name)).ConfigureAwait(false);
        }

        public async Task<ConfigurationReadModel> GetNetworkInformationAsync()
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();
            
            var network = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(data: string.Empty, settings: tomlSettings);

            return network;
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

        private static bool IsValidJson(string strInput)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(strInput, nameof(strInput));

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    //Exception in parsing json
                    return false;
                }
                catch (Exception) //some other exception
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool StartsWithAny(string source, IEnumerable<string> strings)
        {
            foreach (var valueToCheck in strings)
            {
                if (source.StartsWith(valueToCheck))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<List<ModuleReadModel>> GetModuleDataAsync()
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            var fileData = await FileReaderExtensions.ReadAllTextAsync(fileName: string.Empty, folderPath: string.Empty);
            var modules = TomlFileReader.ReadDataAsListFromString<ModuleReadModel>(data: fileData, fieldToRead: "module", settings: tomlSettings);

            if (modules != null && modules.Any())
            {
                foreach (var data in modules)
                {
                    if (!string.IsNullOrWhiteSpace(data.Config))
                    {
                        data.Config = data.Config.Replace("=", ":").Trim().Replace("\r\n", "");

                        if (!StartsWithAny(data.Config, new List<string>() { "{", "[" }))
                        {
                            data.Config = "{" + data.Config + "}";
                        }

                        if (!IsValidJson(data.Config))
                        {
                            data.Config = string.Empty;
                        }
                    }
                }
            }

            return modules;
        }
    }
}
