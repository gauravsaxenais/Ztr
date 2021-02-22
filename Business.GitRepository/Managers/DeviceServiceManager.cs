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
    using ZTR.Framework.Configuration;

    /// <summary>
    /// Device list wrapper for devices.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDeviceServiceManager" />
    public class DeviceServiceManager : ServiceManager, IDeviceServiceManager
    {
        private readonly ILogger<DeviceServiceManager> _logger;
        private const string Prefix = nameof(DeviceServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitConnectionOptions">The git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public DeviceServiceManager(ILogger<DeviceServiceManager> logger, DeviceGitConnectionOptions gitConnectionOptions, IGitRepositoryManager gitRepoManager) : base(logger, gitConnectionOptions, gitRepoManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            _logger = logger;
        }

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        public async Task CloneGitRepoAsync()
        {
            _logger.LogInformation($"Cloning github repository for devices.");
            SetConnection((DeviceGitConnectionOptions)ConnectionOptions);
            await CloneGitHubRepoAsync().ConfigureAwait(false);
            _logger.LogInformation($"Github repository cloning is successful for devices.");
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
        /// Gets the list of devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> GetListOfDevicesAsync()
        {
            string filePath = ((DeviceGitConnectionOptions)ConnectionOptions).DeviceToml;
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();
            var fileContent = await File.ReadAllTextAsync(filePath);

            var fileData = Toml.ReadString(fileContent, tomlSettings);
            var dictionary = fileData.ToDictionary();
            var dictionaryDevices = (Dictionary<string, object>[])dictionary["devices"];

            return dictionaryDevices.ToList();
        }

        protected override void SetupDependencies(GitConnectionOptions connectionOptions)
        {
            var deviceGitConnectionOptions = (DeviceGitConnectionOptions)connectionOptions;
            deviceGitConnectionOptions.DeviceToml = Path.Combine(AppPath, deviceGitConnectionOptions.GitLocalFolder, deviceGitConnectionOptions.DeviceToml);
        }
    }
}
