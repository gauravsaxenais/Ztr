namespace Business.GitRepositoryWrappers.Managers
{
    using Business.GitRepository.Interfaces;
    using Interfaces;
    using Configuration;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;
    using ZTR.Framework.Business.Models;

    /// <summary>
    /// Block Service Manager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IBlockServiceManager" />
    /// <seealso cref="DeviceGitConnectionOptions" />
    public class BlockServiceManager : Manager, IBlockServiceManager, IServiceManager<DeviceGitConnectionOptions>
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DeviceGitConnectionOptions _moduleGitConnectionOptions;
        private readonly ILogger<BlockServiceManager> _logger;
        private const string Prefix = nameof(BlockServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        public BlockServiceManager(ILogger<BlockServiceManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions moduleGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));

            _logger = logger;
            _gitRepoManager = gitRepoManager;
            _moduleGitConnectionOptions = moduleGitConnectionOptions;

            SetGitRepoConnection();
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

            _moduleGitConnectionOptions.BlockConfig = Path.Combine(currentDirectory, _moduleGitConnectionOptions.GitLocalFolder, _moduleGitConnectionOptions.BlockConfig);

            _gitRepoManager.SetConnectionOptions(_moduleGitConnectionOptions);
        }

        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetAllBlockFiles()
        {
            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllBlockFiles)} Getting list of all blocks.");
            var blockConfigDirectory = new DirectoryInfo(_moduleGitConnectionOptions.BlockConfig);
            var filesInDirectory = blockConfigDirectory.EnumerateFiles();

            return filesInDirectory;
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

            _logger.LogInformation(
                $"{Prefix} method name: {nameof(GetAllDevicesAsync)}: Getting list of all directories as devices.");
            var listOfDevices = FileReaderExtensions.GetDirectories(_moduleGitConnectionOptions.DefaultTomlConfiguration.DeviceFolder);
            listOfDevices = listOfDevices.ConvertAll(item => item.ToUpper());

            return listOfDevices;
        }
    }
}
