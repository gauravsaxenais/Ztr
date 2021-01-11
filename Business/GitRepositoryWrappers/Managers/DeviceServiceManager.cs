namespace Business.GitRepositoryWrappers.Managers
{
    using Business.GitRepository.Interfaces;
    using Configuration;
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
    /// Device list wrapper for devices.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDeviceServiceManager" />
    public class DeviceServiceManager : Manager, IDeviceServiceManager, IServiceManager<DevicesGitConnectionOptions>
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DevicesGitConnectionOptions _devicesGitConnectionOptions;
        private readonly ILogger<DeviceServiceManager> _logger;
        private const string Prefix = nameof(DeviceServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        public DeviceServiceManager(ILogger<DeviceServiceManager> logger, IGitRepositoryManager gitRepoManager, DevicesGitConnectionOptions deviceGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _devicesGitConnectionOptions = deviceGitConnectionOptions;

            SetGitRepoConnection();
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            _logger.LogInformation($"{Prefix}: Cloning github repository.");
            await _gitRepoManager.CloneRepositoryAsync().ConfigureAwait(false);
            _logger.LogInformation($"{Prefix}: Github repository cloning is successful.");

            var dictionaryDevices = await GetListOfDevicesAsync().ConfigureAwait(false);

            var listOfDevices = dictionaryDevices.SelectMany(x => x)
                .Where(y => y.Key == "name")
                .Select(z => z.Value.ToString());

            _logger.LogInformation(
                $"{Prefix} method name: {nameof(GetAllDevicesAsync)}: Getting list of all directories as devices.");
            return listOfDevices;
        }

        /// <summary>
        /// Gets the firmware git URL.
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<string> GetFirmwareGitUrlAsync(string deviceType)
        {
            object url = null;
            var dictionaryDevices = await GetListOfDevicesAsync().ConfigureAwait(false);
            
            var device =
                dictionaryDevices.FirstOrDefault(d => d.TryGetValue("name", out object value) 
                                                      && value is string i 
                                                      && string.Equals(i, deviceType,
                                                          StringComparison.OrdinalIgnoreCase));
            device?.TryGetValue("url", out url);

            return url != null ? url.ToString() : string.Empty;
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

            _devicesGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _devicesGitConnectionOptions.GitLocalFolder);
            _devicesGitConnectionOptions.DeviceToml = Path.Combine(_devicesGitConnectionOptions.GitLocalFolder, _devicesGitConnectionOptions.DeviceToml);

            _gitRepoManager.SetConnectionOptions(_devicesGitConnectionOptions);
        }

        private async Task<List<Dictionary<string, object>>> GetListOfDevicesAsync()
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();
            var fileContent
                = await File.ReadAllTextAsync(_devicesGitConnectionOptions.DeviceToml);

            var fileData = Toml.ReadString(fileContent, tomlSettings);

            var dictionary = fileData.ToDictionary();
            var dictionaryDevices = (Dictionary<string, object>[])dictionary["devices"];

            return dictionaryDevices.ToList();
        }
    }
}
