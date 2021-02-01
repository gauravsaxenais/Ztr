namespace Business.GitRepository.Managers
{
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Nett;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration;

    /// <summary>
    /// Device list wrapper for devices.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDeviceServiceManager" />
    public class DeviceServiceManager : Manager, IDeviceServiceManager, IServiceManager
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly ILogger<DeviceServiceManager> _logger;
        private const string Prefix = nameof(DeviceServiceManager);
        private readonly DeviceGitConnectionOptions _devicesGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public DeviceServiceManager(ILogger<DeviceServiceManager> logger, DeviceGitConnectionOptions deviceGitConnectionOptions, IGitRepositoryManager gitRepoManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _devicesGitConnectionOptions = deviceGitConnectionOptions;

            SetGitRepoConnection(_devicesGitConnectionOptions);
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            var dictionaryDevices = await GetListOfDevicesAsync().ConfigureAwait(false);

            var listOfDevices = dictionaryDevices.SelectMany(x => x)
                .Where(y => y.Key == "name")
                .Select(z => z.Value.ToString());

            _logger.LogInformation(
                $"{Prefix} method name: {nameof(GetAllDevicesAsync)}: Getting list of all directories as devices.");
            return listOfDevices;
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
        /// Gets the list of devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> GetListOfDevicesAsync()
        {
            string filePath = _devicesGitConnectionOptions.DeviceToml;
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();
            var fileContent
                = await File.ReadAllTextAsync(filePath);

            var fileData = Toml.ReadString(fileContent, tomlSettings);

            var dictionary = fileData.ToDictionary();
            var dictionaryDevices = (Dictionary<string, object>[])dictionary["devices"];

            return dictionaryDevices.ToList();
        }

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        private void SetGitRepoConnection(GitConnectionOptions connectionOptions)
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (currentDirectory == null)
            {
                throw new CustomArgumentException("Current directory path is not valid.");
            }

            _devicesGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _devicesGitConnectionOptions.GitLocalFolder);
            _devicesGitConnectionOptions.DeviceToml = Path.Combine(_devicesGitConnectionOptions.GitLocalFolder, _devicesGitConnectionOptions.DeviceToml);

            _gitRepoManager.SetConnectionOptions(connectionOptions);
        }
    }
}
