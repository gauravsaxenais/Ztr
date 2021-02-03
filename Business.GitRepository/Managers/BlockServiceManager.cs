namespace Business.GitRepository.Managers
{
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
    using ZTR.Framework.Configuration;

    /// <summary>
    /// BlockServiceManager
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IBlockServiceManager" />
    /// <seealso cref="IServiceManager" />
    public class BlockServiceManager : ServiceManager, IBlockServiceManager
    {
        private readonly ILogger<BlockServiceManager> _logger;
        private const string Prefix = nameof(BlockServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        public BlockServiceManager(ILogger<BlockServiceManager> logger, ModuleBlockGitConnectionOptions moduleGitConnectionOptions, IGitRepositoryManager gitRepoManager) : base(logger, moduleGitConnectionOptions, gitRepoManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            _logger = logger;
        }

        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<FileInfo>> GetAllBlockFilesAsync()
        {
            _logger.LogInformation($"{Prefix}: method name: {nameof(GetAllBlockFilesAsync)} Getting list of all blocks.");
            string blockConfigPath = ((ModuleBlockGitConnectionOptions)ConnectionOptions).BlockConfig;
            var blockConfigDirectory = new DirectoryInfo(blockConfigPath);
            var filesInDirectory = blockConfigDirectory.EnumerateFiles();

            return await Task.FromResult(filesInDirectory);
        }

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task CloneGitRepoAsync()
        {
            await CloneGitHubRepoAsync().ConfigureAwait(false);
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
            var defaultPath = ((ModuleBlockGitConnectionOptions)ConnectionOptions).DefaultTomlConfiguration.DefaultTomlFile;
            var defaultValueFromTomlFile = await GetFileContentFromPath(firmwareVersion, deviceType, defaultPath).ConfigureAwait(false);

            return defaultValueFromTomlFile;
        }

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

        /// <summary>
        /// Setups the dependencies.
        /// </summary>
        protected override void SetupDependencies(GitConnectionOptions connectionOptions)
        {
            var moduleBlockGitConnectionOptions = (ModuleBlockGitConnectionOptions)connectionOptions;

            moduleBlockGitConnectionOptions.BlockConfig = Path.Combine(AppPath,
                moduleBlockGitConnectionOptions.GitLocalFolder, moduleBlockGitConnectionOptions.BlockConfig);
        }
    }
}
