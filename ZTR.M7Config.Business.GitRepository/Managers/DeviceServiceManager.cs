namespace Business.GitRepository.ZTR.M7Config.Business
{
    using EnsureThat;
    using global::ZTR.Framework.Business;
    using global::ZTR.Framework.Business.File.FileReaders;
    using global::ZTR.M7Config.Business.Common.Configuration;
    using global::ZTR.M7Config.Business.GitRepository.Interfaces;
    using Microsoft.Extensions.Logging;
    using Nett;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Device list wrapper for devices.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDeviceServiceManager" />
    public class DeviceServiceManager : IDeviceServiceManager
    {
        private readonly ILogger<DeviceServiceManager> _logger;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;
        private readonly IGitRepositoryManager _repoManager;
        private const string Prefix = nameof(DeviceServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitConnectionOptions">The git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public DeviceServiceManager(ILogger<DeviceServiceManager> logger, DeviceGitConnectionOptions gitConnectionOptions, IGitRepositoryManager gitRepoManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitConnectionOptions, nameof(gitConnectionOptions));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));

            _logger = logger;
            _deviceGitConnectionOptions = gitConnectionOptions;
            _repoManager = gitRepoManager;
        }

        public async Task<string> GetFirmwareGitUrlAsync(string deviceType)
        {
            object url = null;
            var dictionaryDevices = await GetListOfDevicesAsync().ConfigureAwait(false);
            
            var device = dictionaryDevices.FirstOrDefault(d => d.TryGetValue("name", out object value)
                                                      && value is string i
                                                      && string.Equals(i, deviceType,
                                                          StringComparison.OrdinalIgnoreCase));
            device?.TryGetValue("url", out url);
            dictionaryDevices.Clear();
            return url != null ? url.ToString() : string.Empty;
        }
        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        public async Task CloneGitRepoAsync()
        {
            SetConnection();
            _logger.LogInformation($"Cloning github repository for devices.");
            await _repoManager.InitRepositoryAsync().ConfigureAwait(false);
            _logger.LogInformation($"Github repository cloning is successful for devices.");
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            _logger.LogInformation(
                $"{Prefix} method name: {nameof(GetAllDevicesAsync)}: Getting list of all directories as devices.");

            var dictionaryDevices = await GetListOfDevicesAsync().ConfigureAwait(false);
            
            var listOfDevices = dictionaryDevices.SelectMany(x => x)
                .Where(y => y.Key == "name")
                .Select(z => z.Value.ToString());
                        
            return listOfDevices;
        }

        /// <summary>
        /// Gets the list of devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> GetListOfDevicesAsync()
        {
            string filePath = _deviceGitConnectionOptions.DeviceToml;
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();
            var fileContent = await File.ReadAllTextAsync(filePath);

            var fileData = Toml.ReadString(fileContent, tomlSettings);
            var dictionary = fileData.ToDictionary();
            var dictionaryDevices = (Dictionary<string, object>[])dictionary["devices"];

            return dictionaryDevices.ToList();
        }

        /// <summary>
        /// Sets the connection.
        /// </summary>
        /// <param name="connectionOptions">The connection options.</param>
        public void SetConnection()
        {
            _logger.LogInformation("Setting git repository connection");

            var appPath = GlobalMethods.GetCurrentAppPath();
            _deviceGitConnectionOptions.DeviceToml = Path.Combine(appPath, _deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.DeviceToml);
            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(appPath, _deviceGitConnectionOptions.GitLocalFolder);
            _repoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }
    }
}
