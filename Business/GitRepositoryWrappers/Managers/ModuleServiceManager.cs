namespace Business.GitRepositoryWrappers.Managers
{
    using Business.GitRepository.Interfaces;
    using Business.RequestHandlers.Managers;
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
    public class ModuleServiceManager : Manager, IModuleServiceManager, IServiceManager<ModuleBlockGitConnectionOptions>
    {
        private readonly string protoFileName = "module.proto";
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly ModuleBlockGitConnectionOptions _moduleGitConnectionOptions;
        private readonly ILogger<ModuleServiceManager> _logger;
        private const string Prefix = nameof(ModuleServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        public ModuleServiceManager(ILogger<ModuleServiceManager> logger, IGitRepositoryManager gitRepoManager, ModuleBlockGitConnectionOptions moduleGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _moduleGitConnectionOptions = moduleGitConnectionOptions;
            
            SetGitRepoConnection();
        }

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        public void SetGitRepoConnection()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _moduleGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder);
            _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder = Path.Combine(_moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
            _moduleGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.ModulesConfig);

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
            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllModulesAsync)} Getting list of all modules {firmwareVersion} {deviceType}.");
            var listOfModules = await GetListOfModulesAsync(firmwareVersion, deviceType).ConfigureAwait(false);

            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllModulesAsync)} Modules retrieved for {firmwareVersion} {deviceType}. Getting icons for modules...");
            foreach (var module in listOfModules)
            {
                module.IconUrl = GetModuleIconUrl(module);
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
        /// Gets the default content of the toml file.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<string> GetDefaultTomlFileContentAsync(string firmwareVersion, string deviceType)
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetDefaultTomlFileContentAsync)}: Getting default value from toml file for {firmwareVersion}, {deviceType}.");
            var defaultValueFromTomlFile = await GetFileContentFromPath(firmwareVersion, deviceType, _moduleGitConnectionOptions.DefaultTomlConfiguration.DefaultTomlFile).ConfigureAwait(false);

            return defaultValueFromTomlFile;
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
        /// Gets the list of modules.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        private async Task<List<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType)
        {
            var listOfModules = new List<ModuleReadModel>();
            var fileContent = await GetFileContentFromPath(firmwareVersion, deviceType,
                _moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceTomlFile)
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
