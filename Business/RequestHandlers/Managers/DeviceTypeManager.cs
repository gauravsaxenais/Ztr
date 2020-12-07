namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// Returns device information.
    /// </summary>
    /// <seealso cref="ZTR.Framework.Business.Manager" />
    /// <seealso cref="Business.RequestHandlers.Interfaces.IDeviceTypeManager" />
    public class DeviceTypeManager : Manager, IDeviceTypeManager
    {
        private readonly IGitRepositoryManager _repoManager;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTypeManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        public DeviceTypeManager(ILogger<DeviceTypeManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions deviceGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));

            _repoManager = gitRepoManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;

            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);
            _deviceGitConnectionOptions.TomlConfiguration.DeviceFolder = Path.Combine(_deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.TomlConfiguration.DeviceFolder);

            _repoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            var listOfDevices = new List<string>();

            await _repoManager.CloneRepositoryAsync();
            var deviceGitConnection = (DeviceGitConnectionOptions)_repoManager.GetConnectionOptions();

            if (deviceGitConnection != null)
            {
                listOfDevices = FileReaderExtensions.GetDirectories(deviceGitConnection.TomlConfiguration.DeviceFolder);
                listOfDevices = listOfDevices.ConvertAll(item => item.ToUpper());
            }

            return listOfDevices;
        }

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync()
        {
            var listFirmwareVersions = await _repoManager.LoadTagNamesAsync();

            return listFirmwareVersions;
        }
    }
}
