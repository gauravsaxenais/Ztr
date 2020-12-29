namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// This manager takes input of firmware version
    /// and returns list of firmware versions as an array.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="ICompatibleFirmwareVersionManager" />
    public class CompatibleFirmwareVersionManager : Manager, ICompatibleFirmwareVersionManager
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigCreateFromManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        public CompatibleFirmwareVersionManager(ILogger<DefaultValueManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions deviceGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));
            EnsureArg.IsNotNull(deviceGitConnectionOptions.DefaultTomlConfiguration, nameof(deviceGitConnectionOptions.DefaultTomlConfiguration));

            _gitRepoManager = gitRepoManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;
        }

        /// <summary>
        /// Generates the configuration toml model asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns>
        /// ApiResponse - success: true/false. data: array of compatible firmware versions.
        /// </returns>
        public async Task<ApiResponse> GetCompatibleFirmwareVersionsAsync(string firmwareVersion)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(firmwareVersion);
            var prefix = nameof(CompatibleFirmwareVersionManager);
            ApiResponse apiResponse = null;

            try
            {
                Logger.LogInformation($"{prefix}: Getting list of compatible firmware versions based on a firmware version.");

                SetGitRepoConnections();

                await _gitRepoManager.CloneRepositoryAsync();

                apiResponse = new ApiResponse(status: true, data: null);
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, $"{prefix}: Error occured while getting list of compatible firmware versions based on a firmware version.");
                apiResponse = new ApiResponse(false, exception.Message, ErrorType.BusinessError, exception);
            }

            return apiResponse;
        }

        private void SetGitRepoConnections()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);
            _deviceGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.ModulesConfig);

            _gitRepoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }
    }
}
