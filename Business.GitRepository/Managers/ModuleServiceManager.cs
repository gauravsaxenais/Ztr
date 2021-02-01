namespace Business.GitRepository.Managers
{
    using Business.Common.Models;
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
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
    public class ModuleServiceManager : Manager, IModuleServiceManager, IServiceManager
    {
        private readonly string protoFileName = "module.proto";
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly ModuleBlockGitConnectionOptions _moduleGitConnectionOptions;
        private readonly ILogger<ModuleServiceManager> _logger;
        private const string Prefix = nameof(ModuleServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public ModuleServiceManager(ILogger<ModuleServiceManager> logger, ModuleBlockGitConnectionOptions moduleGitConnectionOptions, IGitRepositoryManager gitRepoManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _moduleGitConnectionOptions = moduleGitConnectionOptions;

            SetGitRepoConnection(_moduleGitConnectionOptions);
        }

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        public void SetGitRepoConnection(ModuleBlockGitConnectionOptions moduleGitConnectionOptions)
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _moduleGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder);
            _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder = Path.Combine(_moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
            _moduleGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.ModulesConfig);

            _gitRepoManager.SetConnectionOptions(moduleGitConnectionOptions);
        }

        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<List<ModuleReadModel>> GetAllModulesAsync(string firmwareVersion, string deviceType)
        {
            string moduleFilePath = _moduleGitConnectionOptions.ModulesConfig;
            string deviceTomlFilePath = _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceTomlFile;
            string metaTomlFilePath = _moduleGitConnectionOptions.MetaToml;

            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllModulesAsync)} Getting list of all modules {firmwareVersion} {deviceType}.");
            var listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType, deviceTomlFilePath).ConfigureAwait(false);

            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllModulesAsync)} Modules retrieved for {firmwareVersion} {deviceType}. Getting icons for modules...");

            foreach (var module in listOfModules)
            {
                module.IconUrl = GetModuleIconUrl(module, moduleFilePath, metaTomlFilePath);
            }

            // fix the indexes.
            listOfModules.Select((item, index) => { item.Id = index; return item; }).ToList();
            return listOfModules;
        }

        /// <summary>
        /// Gets the tags earlier than this tag.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns></returns>
        public async Task<List<string>> GetTagsEarlierThanThisTagAsync(string firmwareVersion)
        {
            var listOfTags = await _gitRepoManager.GetTagsEarlierThanThisTagAsync(firmwareVersion).ConfigureAwait(false);

            return listOfTags;
        }

        /// <summary>
        /// Gets the default toml file content asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<string> GetDefaultTomlFileContentAsync(string firmwareVersion, string deviceType)
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetDefaultTomlFileContentAsync)}: Getting default value from toml file for {firmwareVersion}, {deviceType}.");
            var defaultPath = _moduleGitConnectionOptions.DefaultTomlConfiguration.DefaultTomlFile;
            var defaultValueFromTomlFile = await GetFileContentFromPath(firmwareVersion, deviceType, defaultPath).ConfigureAwait(false);

            return defaultValueFromTomlFile;
        }

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        public async Task CloneGitHubRepoAsync()
        {
            _logger.LogInformation($"{Prefix}: Cloning github repository.");
            await _gitRepoManager.CloneRepositoryAsync().ConfigureAwait(false);
            _logger.LogInformation($"{Prefix}: Github repository cloning is successful.");
        }

        /// <summary>
        /// Gets the proto files.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        public string GetProtoFiles(ModuleReadModel module)
        {
            EnsureArg.IsNotNull(module);

            string moduleFilePath = _moduleGitConnectionOptions.ModulesConfig;
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
        /// Gets the module icon URL.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="moduleFilePath">The module file path.</param>
        /// <param name="metaTomlFilePath">The meta toml file path.</param>
        /// <returns></returns>
        private string GetModuleIconUrl(ModuleReadModel module, string moduleFilePath, string metaTomlFilePath)
        {
            EnsureArg.IsNotNull(module);
            var iconUrl = string.Empty;

            var moduleFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFilePath, module.Name);

            if (string.IsNullOrWhiteSpace(moduleFolder))
            {
                return string.Empty;
            }

            var metaTomlFile = Path.Combine(moduleFolder, metaTomlFilePath);

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
                        if (moduleFromToml != null && (string)moduleFromToml["name"] == module.Name)
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
        /// Gets the list of modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="deviceTomlFilePath">The device toml file path.</param>
        /// <returns></returns>
        private async Task<List<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType, string deviceTomlFilePath)
        {
            var listOfModules = new List<ModuleReadModel>();
            var fileContent = await GetFileContentFromPath(firmwareVersion, deviceType, deviceTomlFilePath)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                var data = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(data: fileContent);

                listOfModules = data.Module;
            }

            return listOfModules;
        }

        private async Task<string> GetFileContentFromPath(string firmwareVersion, string deviceType, string path)
        {
            var listOfFiles = await _gitRepoManager
                .GetFileDataFromTagAsync(firmwareVersion, path)
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
