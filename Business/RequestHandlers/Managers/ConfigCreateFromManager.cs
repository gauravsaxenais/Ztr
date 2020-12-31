namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// This manager takes config.toml as input and returns
    /// list of modules and blocks.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IConfigCreateFromManager" />
    public class ConfigCreateFromManager : Manager, IConfigCreateFromManager
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;
        private readonly IDefaultValueManager _defaultValueManager;
        private readonly IBlockManager _blockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigCreateFromManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="defaultValueManager">The default value manager.</param>
        /// <param name="blockManager">The block manager.</param>
        /// <param name="deviceGitConnectionOptions">The device git connection options.</param>
        public ConfigCreateFromManager(ILogger<DefaultValueManager> logger, IGitRepositoryManager gitRepoManager, IDefaultValueManager defaultValueManager, IBlockManager blockManager, DeviceGitConnectionOptions deviceGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));
            EnsureArg.IsNotNull(defaultValueManager, nameof(defaultValueManager));
            EnsureArg.IsNotNull(blockManager, nameof(blockManager));

            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));
            EnsureArg.IsNotNull(deviceGitConnectionOptions.DefaultTomlConfiguration, nameof(deviceGitConnectionOptions.DefaultTomlConfiguration));

            _defaultValueManager = defaultValueManager;
            _blockManager = blockManager;
            _gitRepoManager = gitRepoManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;
        }

        /// <summary>
        /// Returns the list of all modules and blocks from config.toml.
        /// </summary>
        /// <param name="configTomlFile">config.toml as string.</param>
        /// <returns></returns>
        public async Task<ApiResponse> GenerateConfigTomlModelAsync(IFormFile configTomlFile)
        {
            EnsureArg.IsNotNull(configTomlFile);
            var prefix = nameof(ConfigCreateFromManager);
            ApiResponse apiResponse = null;

            try
            {
                Logger.LogInformation($"{prefix}: Getting list of modules and blocks from config.toml file.");

                SetGitRepoConnections();

                await _gitRepoManager.CloneRepositoryAsync().ConfigureAwait(false);

                var configTomlFileContent = ReadAsString(configTomlFile);

                // get list of all modules.
                var modules = GetListOfModules(configTomlFileContent);

                await _defaultValueManager.MergeValuesWithModulesAsync(configTomlFileContent, modules, _deviceGitConnectionOptions.ModulesConfig);
                
                var blocksDirectory = new DirectoryInfo(_deviceGitConnectionOptions.BlockConfig);
                var blocks = _blockManager.GetListOfBlocks(blocksDirectory);

                apiResponse = new ApiResponse(status: true, data: new { modules, blocks });
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, $"{prefix}: Error occured while getting list of modules and blocks from toml file.");
                apiResponse = new ApiResponse(false, exception.Message, ErrorType.BusinessError, exception);
            }

            return apiResponse;
        }

        private string ReadAsString(IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(reader.ReadLine());
                }
            }

            return result.ToString();
        }

        private void SetGitRepoConnections()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);
            _deviceGitConnectionOptions.ModulesConfig = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder, _deviceGitConnectionOptions.ModulesConfig);

            _gitRepoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }

        /// <summary>
        /// Gets the list of modules.
        /// </summary>
        /// <param name="configTomlFile">The configuration toml file.</param>
        /// <returns></returns>
        private IEnumerable<ModuleReadModel> GetListOfModules(string configTomlFile)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(configTomlFile);

            var listOfModules = new List<ModuleReadModel>();

            var data = GetTomlData(configTomlFile);

            listOfModules = data.Module;

            listOfModules = listOfModules.Select((module, index) => new ModuleReadModel { Id = index, Config = module.Config, Name = module.Name, UUID = module.UUID }).ToList();

            return listOfModules;
        }

        /// <summary>
        /// Gets the toml data.
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <returns></returns>
        private ConfigurationReadModel GetTomlData(string fileContent)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            var tomlData = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(data: fileContent, settings: tomlSettings);

            return tomlData;
        }
    }
}
