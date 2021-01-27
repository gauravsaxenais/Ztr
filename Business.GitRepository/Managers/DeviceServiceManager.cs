namespace Business.GitRepository.Managers
{
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

    /// <summary>
    /// Device list wrapper for devices.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IDeviceServiceManager" />
    public class DeviceServiceManager : Manager, IDeviceServiceManager, IServiceManager<GitConnectionOptions>
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly ILogger<DeviceServiceManager> _logger;
        private const string Prefix = nameof(DeviceServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public DeviceServiceManager(ILogger<DeviceServiceManager> logger, IGitRepositoryManager gitRepoManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync(string filePath)
        {
            var dictionaryDevices = await GetListOfDevicesAsync(filePath).ConfigureAwait(false);

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
        /// Sets the git repo connection.
        /// </summary>
        /// <exception cref="CustomArgumentException">Current directory path is not valid.</exception>
        public void SetGitRepoConnection(GitConnectionOptions connectionOptions)
        {
            _gitRepoManager.SetConnectionOptions(connectionOptions);
        }

        public async Task<List<Dictionary<string, object>>> GetListOfDevicesAsync(string filePath)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettings();
            var fileContent
                = await File.ReadAllTextAsync(filePath);

            var fileData = Toml.ReadString(fileContent, tomlSettings);

            var dictionary = fileData.ToDictionary();
            var dictionaryDevices = (Dictionary<string, object>[])dictionary["devices"];

            return dictionaryDevices.ToList();
        }
    }
}
