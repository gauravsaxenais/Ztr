namespace Business.RequestHandlers.Managers
{
    using Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using Nett;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;
    using ZTR.Framework.Business.Models;

    /// <summary>
    /// Wrapper for GitRepoManager.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleServiceManager" />
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleServiceManager" />
    public class ModuleServiceManager : Manager, IModuleServiceManager
    {
        private readonly string protoFileName = "module.proto";
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DeviceGitConnectionOptions _moduleGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        public ModuleServiceManager(ILogger<ModuleServiceManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions moduleGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));

            _gitRepoManager = gitRepoManager;
            _moduleGitConnectionOptions = moduleGitConnectionOptions;
            
            SetGitRepoConnection();
        }

        private void SetGitRepoConnection()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _moduleGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder);
            _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder = Path.Combine(_moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
            _moduleGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.ModulesConfig);
            _moduleGitConnectionOptions.BlockConfig = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.BlockConfig);

            _gitRepoManager.SetConnectionOptions(_moduleGitConnectionOptions);
        }

        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<List<ModuleReadModel>> GetAllModulesAsync(string firmwareVersion, string deviceType)
        {
            var listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType);

            foreach (var module in listOfModules)
            {
                module.IconUrl = GetModuleIconUrl(module);
            }

            // fix the indexes.
            listOfModules.Select((item, index) => { item.Id = index; return item; }).ToList();
            return listOfModules;
        }

        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetAllBlockFiles()
        {
            var blockConfigDirectory = new DirectoryInfo(_moduleGitConnectionOptions.BlockConfig);
            var filesInDirectory = blockConfigDirectory.EnumerateFiles();

            return filesInDirectory;
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            await _gitRepoManager.CloneRepositoryAsync().ConfigureAwait(false);

            var listOfDevices = FileReaderExtensions.GetDirectories(_moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
            listOfDevices = listOfDevices.ConvertAll(item => item.ToUpper());

            return listOfDevices;
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync()
        {
            var listFirmwareVersions = await _gitRepoManager.GetAllTagNamesAsync();

            return listFirmwareVersions;
        }

        /// <summary>
        /// Gets the tags earlier than this tag asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns></returns>
        public async Task<List<string>> GetTagsEarlierThanThisTagAsync(string firmwareVersion)
        {
            var listOfTags = await _gitRepoManager.GetTagsEarlierThanThisTagAsync(firmwareVersion);

            return listOfTags;
        }

        /// <summary>
        /// Gets the default content of the toml file.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<string> GetDefaultTomlFileContent(string firmwareVersion, string deviceType)
        {
            var defaultValueFromTomlFile = await GetFileContentFromPath(firmwareVersion, deviceType, _moduleGitConnectionOptions.DefaultTomlConfiguration.DefaultTomlFile);

            return defaultValueFromTomlFile;
        }

        /// <summary>
        /// Gets the module icon URL.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        private string GetModuleIconUrl(ModuleReadModel module)
        {
            EnsureArg.IsNotNull(module);
            var iconUrl = string.Empty;
            var moduleFilePath = _moduleGitConnectionOptions.ModulesConfig;

            var moduleFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFilePath, module.Name);

            if (string.IsNullOrWhiteSpace(moduleFolder))
            {
                return string.Empty;
            }

            var metaTomlFile = Path.Combine(moduleFolder, _moduleGitConnectionOptions.MetaToml);

            try
            {
                if (File.Exists(metaTomlFile))
                {
                    var tml = Toml.ReadFile(metaTomlFile, TomlFileReader.LoadLowerCaseTomlSettings());

                    var dict = tml.ToDictionary();
                    var moduleValues = dict["module"];

                    if (moduleValues is Dictionary<string, object>)
                    {
                        var moduleFromToml = (Dictionary<string, object>)dict["module"];
                        if (moduleFromToml != null && (string) moduleFromToml["name"] == module.Name)
                        {
                            iconUrl = moduleFromToml["iconUrl"].ToString();

                            if (!iconUrl.IsPathUrl())
                            {
                                return string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                iconUrl = string.Empty;
            }

            return iconUrl;
        }

        /// <summary>
        /// Gets the proto files.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        public string GetProtoFiles(ModuleReadModel module)
        {
            EnsureArg.IsNotNull(module);

            var moduleFilePath = _moduleGitConnectionOptions.ModulesConfig;

            var moduleFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFilePath, module.Name);

            if (string.IsNullOrWhiteSpace(moduleFolder))
            {
                return string.Empty;
            }

            var uuidFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFolder, module.UUID);

            if (!string.IsNullOrWhiteSpace(uuidFolder))
            {
                foreach (var file in Directory.EnumerateFiles(uuidFolder, protoFileName))
                {
                    return file;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the list of modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        private async Task<List<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType)
        {
            var listOfModules = new List<ModuleReadModel>();
            var fileContent = await GetFileContentFromPath(firmwareVersion, deviceType, _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceTomlFile);

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                var data = GetTomlData(fileContent);

                listOfModules = data.Module;
            }

            return listOfModules;
        }

        private ConfigurationReadModel GetTomlData(string fileContent)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();
            var tomlData = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(data: fileContent, settings: tomlSettings);

            return tomlData;
        }

        private async Task<string> GetFileContentFromPath(string firmwareVersion, string deviceType, string path)
        {
            var gitConnectionOptions = (DeviceGitConnectionOptions)_gitRepoManager.GetConnectionOptions();

            var listOfFiles = await _gitRepoManager.GetFileDataFromTagAsync(firmwareVersion, path)
                                                   .ConfigureAwait(false);

            // case insensitive search.
            var deviceTypeFile = listOfFiles.FirstOrDefault(p => p.FileName?.IndexOf(deviceType, StringComparison.OrdinalIgnoreCase) >= 0);

            var fileContent = string.Empty;

            if (deviceTypeFile != null)
            {
                fileContent = System.Text.Encoding.UTF8.GetString(deviceTypeFile.Data);
            }

            return fileContent;
        }
    }
}
