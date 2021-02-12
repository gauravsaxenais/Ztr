namespace Business.GitRepository.Managers
{
    using Business.Common.Models;
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;
    using ZTR.Framework.Configuration;

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
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync()
        {
            _logger.LogInformation(
                $"{Prefix} method name: {nameof(GetAllFirmwareVersionsAsync)}: Getting list of all firmware versions for deviceType.");
            var firmwareVersions = await RepoManager.GetAllTagNamesAsync().ConfigureAwait(false);

            return firmwareVersions;
        }

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        public async Task CloneGitRepoAsync()
        {
            SetConnection((FirmwareVersionGitConnectionOptions)ConnectionOptions);
            await CloneGitHubRepoAsync().ConfigureAwait(false);
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
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        public async Task<string> GetDefaultTomlFileContentAsync(string firmwareVersion, string deviceType)
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetDefaultTomlFileContentAsync)}: Getting default value from toml file for {firmwareVersion}, {deviceType}.");
            var defaultPath = ((FirmwareVersionGitConnectionOptions)ConnectionOptions).DefaultTomlConfiguration.DefaultTomlFile;
            var defaultValueFromTomlFile = await GetFileContentFromPath(firmwareVersion, deviceType, defaultPath).ConfigureAwait(false);

            return defaultValueFromTomlFile;
        }

        public async Task<string> GetDeviceTomlFileContentAsync(string firmwareVersion, string deviceType)
        {
            _logger.LogInformation($"{Prefix} method name: {nameof(GetDeviceTomlFileContentAsync)}: Getting device value from toml file for {firmwareVersion}, {deviceType}.");

            var devicesPath = ((FirmwareVersionGitConnectionOptions)ConnectionOptions).DefaultTomlConfiguration.DeviceTomlFile;
            var deviceValueFromTomlFile = await GetFileContentFromPath(firmwareVersion, deviceType, devicesPath).ConfigureAwait(false);

            return deviceValueFromTomlFile;
        }

        /// <summary>
        /// Gets the tags earlier than this tag.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns></returns>
        public async Task<List<string>> GetTagsEarlierThanThisTagAsync(string firmwareVersion)
        {
            var listOfTags = await RepoManager.GetTagsEarlierThanThisTagAsync(firmwareVersion).ConfigureAwait(false);
            return listOfTags;
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
            var fileContent = await GetDeviceTomlFileContentAsync(firmwareVersion, deviceType)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                var data = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(data: fileContent);
                listOfModules = data.Module;
            }

            // fix the indexes.
            listOfModules.Select((item, index) => { item.Id = index; return item; }).ToList();

            return listOfModules;
        }


        /// <summary>
        /// Gets the file content from path.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private async Task<string> GetFileContentFromPath(string firmwareVersion, string deviceType, string path)
        {
            var listOfFiles = await RepoManager
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

        protected override void SetupDependencies(GitConnectionOptions connectionOptions)
        {
            var firmwareVersionGitConnectionOptions = (FirmwareVersionGitConnectionOptions)connectionOptions;
            firmwareVersionGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder = Path.Combine(AppPath, firmwareVersionGitConnectionOptions.GitLocalFolder, firmwareVersionGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
        }
    }
}
