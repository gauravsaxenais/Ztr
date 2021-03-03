namespace Business.GitRepository.Managers
{
    using Business.Common.Models;
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// FirmwareVersion Service Manager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IFirmwareVersionServiceManager" />
    /// <seealso cref="IServiceManager" />
    public class FirmwareVersionServiceManager : ServiceManager, IFirmwareVersionServiceManager
    {
        private readonly ILogger<FirmwareVersionServiceManager> _logger;
        private const string Prefix = nameof(FirmwareVersionServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="firmwareVersionGitConnection">The firmware version git connection.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceServiceManager">The device service manager.</param>
        public FirmwareVersionServiceManager(ILogger<FirmwareVersionServiceManager> logger, FirmwareVersionGitConnectionOptions firmwareVersionGitConnection, IGitRepositoryManager gitRepoManager) : base(logger, firmwareVersionGitConnection, gitRepoManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            _logger = logger;
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllFirmwareVersionsAsync()
        {
            _logger.LogInformation(
                $"{Prefix} method name: {nameof(GetAllFirmwareVersionsAsync)}: Getting list of all firmware versions for deviceType.");
            var specConfigFolder = ((FirmwareVersionGitConnectionOptions)ConnectionOptions).TomlConfiguration.TomlConfigFolder;
            var firmwareVersions = await RepoManager.GetAllTagNamesAsync().ConfigureAwait(false);
            Parallel.ForEach(firmwareVersions, async firmwareVersion =>
            {
                var isPresent = await RepoManager.IsFolderPresentInTag(firmwareVersion.Key, specConfigFolder).ConfigureAwait(false);
                if (!isPresent)
                {
                    firmwareVersions.Remove(firmwareVersion.Key);
                }
            });

            var firmwareVersionsWithSpecFolder = firmwareVersions.OrderByDescending(item => item.Value).Select(item => item.Key).ToList();
            return firmwareVersionsWithSpecFolder;
        }

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        public async Task CloneGitRepoAsync()
        {
            SetConnection((FirmwareVersionGitConnectionOptions)ConnectionOptions);
            _logger.LogInformation($"Cloning github repository for firmware version.");
            await CloneGitHubRepoAsync().ConfigureAwait(false);
            _logger.LogInformation($"Github repository cloning is successful for firmware version.");
        }

        /// <summary>
        /// Sets the git repo URL.
        /// </summary>
        /// <param name="gitUrl">The git URL.</param>
        public void SetGitRepoUrl(string gitUrl)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(gitUrl);
            ConnectionOptions.GitRemoteLocation = gitUrl;
            RepoManager.SetConnectionOptions(ConnectionOptions);
        }

        /// <summary>
        /// Gets the default toml file content asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns></returns>
        public async Task<string> GetDefaultTomlFileContentAsync(string firmwareVersion)
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetDefaultTomlFileContentAsync)}: Getting default value from toml file for {firmwareVersion}.");
            var firmwareVersionConnectionOptions = (FirmwareVersionGitConnectionOptions)ConnectionOptions;
            var defaultPath = Path.Combine(firmwareVersionConnectionOptions.TomlConfiguration.TomlConfigFolder, firmwareVersionConnectionOptions.TomlConfiguration.DefaultTomlFile);
            var defaultValueFromTomlFile = await GetFileContentFromPath(firmwareVersion, defaultPath).ConfigureAwait(false);
            _logger.LogInformation($"{Prefix} method name: {nameof(GetDefaultTomlFileContentAsync)}: Successfully retrieved default value from toml file for {firmwareVersion}.");

            return defaultValueFromTomlFile;
        }

        public async Task<string> GetDeviceTomlFileContentAsync(string firmwareVersion)
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetDeviceTomlFileContentAsync)}: Getting device value from toml file for {firmwareVersion}.");
            var firmwareVersionConnectionOptions = (FirmwareVersionGitConnectionOptions)ConnectionOptions;
            var devicesPath = Path.Combine(firmwareVersionConnectionOptions.TomlConfiguration.TomlConfigFolder, firmwareVersionConnectionOptions.TomlConfiguration.DeviceTomlFile);
            var deviceValueFromTomlFile = await GetFileContentFromPath(firmwareVersion, devicesPath).ConfigureAwait(false);
            return deviceValueFromTomlFile;
        }

        public async Task<List<string>> GetCompatibleFirmwareVersions(List<string> tagList, string mainTag, string deviceType, List<ModuleReadModel> mainModuleList)
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetCompatibleFirmwareVersions)}: Getting list of compatible firmware versions for selected firmware: {mainTag} and deviceType: {deviceType}.");
            var firmwareVersionConnectionOptions = (FirmwareVersionGitConnectionOptions)ConnectionOptions;
            var devicesPath = Path.Combine(firmwareVersionConnectionOptions.TomlConfiguration.TomlConfigFolder, firmwareVersionConnectionOptions.TomlConfiguration.DeviceTomlFile);
            var finalList = new List<string>();
            bool valid = true;
            var main = mainTag;
            string current;

            for (int index = 0; index < tagList.Count; index++)
            {
                current = tagList[index];

                bool changed = IsDeviceFileChanged(current, main, devicesPath);
                if (!changed)
                {
                    if (valid)
                    {
                        finalList.Add(current);
                    }
                }
                else
                {
                    valid = await IsCompatibleFirmwareVersionAsync(mainModuleList, current, deviceType);
                    if (valid)
                    {
                        finalList.Add(current);
                    }
                    main = current;
                }
            }

            return finalList;
        }

        /// <summary>
        /// Gets the list of modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="deviceTomlFilePath">The device toml file path.</param>
        /// <returns></returns>
        public async Task<List<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType)
        {
            var listOfModules = new List<ModuleReadModel>();
            var fileContent = await GetDeviceTomlFileContentAsync(firmwareVersion)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                var data = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(data: fileContent);
                listOfModules = data.Module;
            }

            // fix the indexes.
            listOfModules = listOfModules.Select((item, index) => { item.Id = index; return item; }).ToList();

            return listOfModules;
        }

        private async Task<bool> IsCompatibleFirmwareVersionAsync(List<ModuleReadModel> mainFirmwareVersionModules, string current, string deviceType)
        {
            var modules = await GetListOfModulesAsync(current, deviceType);
            return mainFirmwareVersionModules.Intersect(modules, new ModuleReadModelComparer()).Count() == mainFirmwareVersionModules.Count;
        }

        private bool IsDeviceFileChanged(string fromTag, string toTag, string devicesPath) => RepoManager.IsFileChangedBetweenTags(fromTag, toTag, devicesPath);

        /// <summary>
        /// Gets the file content from path.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private async Task<string> GetFileContentFromPath(string firmwareVersion, string path)
        {
            var fileContent = string.Empty;
            var file = await RepoManager.GetFileDataFromTagAsync(firmwareVersion, path).ConfigureAwait(false);
            if (file != null)
            {
                fileContent = System.Text.Encoding.UTF8.GetString(file.Data);
            }
            return fileContent;
        }
    }
}
