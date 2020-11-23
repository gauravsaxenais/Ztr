namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    public class DeviceTypeManager : Manager, IDeviceTypeManager
    {
        private readonly IGitRepositoryManager _repoManager;
        
        public DeviceTypeManager(ILogger<DeviceTypeManager> logger, IGitRepositoryManager gitRepoManager, IEnvironmentSettings environmentSettings) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(environmentSettings, nameof(environmentSettings));

            _repoManager = gitRepoManager;

            var environmentOptions = environmentSettings.GetDeviceGitConnectionOptions();
            _repoManager.SetConnectionOptions(environmentOptions);
        }

        public async Task<IEnumerable<string>> GetAllDevicesAsync()
        {
            var listOfDevices = new List<string>();

            await _repoManager.CloneRepositoryAsync();
            var gitConnection = _repoManager.GetConnectionOptions();

            if (gitConnection != null)
            {
                listOfDevices = FileReaderExtensions.GetDirectories(gitConnection.TomlConfiguration.DeviceFolder);
                listOfDevices = listOfDevices.ConvertAll(item => item.ToUpper());
            }

            return listOfDevices;
        }

        public async Task<IEnumerable<string>> GetAllFirmwareVersionsAsync()
        {
            var listFirmwareVersions = await _repoManager.LoadTagNamesAsync();
            
            return listFirmwareVersions;
        }
    }
}
