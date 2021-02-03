namespace Business.RequestHandlers.Managers
{
    using Business.Common.Models;
    using Business.GitRepository.Interfaces;
    using EnsureThat;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Models;
    using System.Collections.Generic;
    using System.Linq;
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
        private readonly IDefaultValueManager _defaultValueManager;
        private readonly IBlockManager _blockManager;
        private readonly ILogger _logger;
        private readonly IModuleServiceManager _moduleServiceManager;
        private const string Prefix = nameof(ConfigCreateFromManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigCreateFromManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleServiceManager">The module service manager.</param>
        /// <param name="defaultValueManager">The default value manager.</param>
        /// <param name="blockManager">The block manager.</param>
        public ConfigCreateFromManager(ILogger<ConfigCreateFromManager> logger, IModuleServiceManager moduleServiceManager, IDefaultValueManager defaultValueManager, IBlockManager blockManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(defaultValueManager, nameof(defaultValueManager));
            EnsureArg.IsNotNull(blockManager, nameof(blockManager));
            EnsureArg.IsNotNull(moduleServiceManager, nameof(moduleServiceManager));

            _blockManager = blockManager;
            _defaultValueManager = defaultValueManager;
            _logger = logger;
            _moduleServiceManager = moduleServiceManager;
        }

        /// <summary>
        /// Returns the list of all modules and blocks from config.toml.
        /// </summary>
        /// <param name="configTomlFile">config.toml as string.</param>
        /// <returns></returns>
        public async Task<object> GenerateConfigTomlModelAsync(IFormFile configTomlFile)
        {
            EnsureArg.IsNotNull(configTomlFile);           
            var configTomlFileContent = FileReaderExtensions.ReadAsString(configTomlFile);
            return await GenerateConfigTomlModelAsync(configTomlFileContent);
        }

        /// <summary>
        /// Returns the list of all modules and blocks from config.toml.
        /// </summary>
        /// <param name="configTomlFileContent">config.toml as string.</param>
        /// <returns></returns>
        public async Task<object> GenerateConfigTomlModelWithoutGitAsync(string configTomlFileContent)
        {
            var prefix = nameof(ConfigCreateFromManager);
            _logger.LogInformation($"{prefix}: methodName: {nameof(GenerateConfigTomlModelAsync)} Getting list of modules and blocks from config.toml file.");

            var modules = await GetModulesAsync(configTomlFileContent).ConfigureAwait(false);
            var blocks = await GetBlocksAsync(configTomlFileContent).ConfigureAwait(false);

            return new { modules, blocks };
        }

        /// <summary>
        /// Returns the list of all modules and blocks from config.toml.
        /// </summary>
        /// <param name="configTomlFileContent">config.toml as string.</param>
        /// <returns></returns>
        public async Task<object> GenerateConfigTomlModelAsync(string configTomlFileContent)
        {            
            var prefix = nameof(ConfigCreateFromManager);
            _logger.LogInformation($"{prefix}: methodName: {nameof(GenerateConfigTomlModelAsync)} Getting list of modules and blocks from config.toml file.");

            // Clone repo here.
            await _moduleServiceManager.CloneGitRepoAsync().ConfigureAwait(false);
            var data = await GenerateConfigTomlModelWithoutGitAsync(configTomlFileContent).ConfigureAwait(false);

            return data;
        }

        /// <summary>
        /// Gets the modules asynchronous.
        /// </summary>
        /// <param name="configTomlFileContent">Content of the configuration toml file.</param>
        /// <returns></returns>
        private async Task<IEnumerable<ModuleReadModel>> GetModulesAsync(string configTomlFileContent)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(configTomlFileContent);

            // get list of all modules.
            var modules = GetListOfModules(configTomlFileContent).ToList();
            await _defaultValueManager.MergeValuesWithModulesAsync(configTomlFileContent, modules).ConfigureAwait(false);

            return modules;
        }

        /// <summary>
        /// Gets the blocks asynchronous.
        /// </summary>
        /// <param name="configTomlFileContent">Content of the configuration toml file.</param>
        /// <returns></returns>
        private async Task<IEnumerable<BlockJsonModel>> GetBlocksAsync(string configTomlFileContent)
        {
            var blocks = await _blockManager.GetBlocksFromFileAsync(configTomlFileContent).ConfigureAwait(false);
            return blocks;
        }

        /// <summary>
        /// Gets the list of modules.
        /// </summary>
        /// <param name="configTomlFile">The configuration toml file.</param>
        /// <returns></returns>
        private IEnumerable<ModuleReadModel> GetListOfModules(string configTomlFile)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(configTomlFile);

            var data = TomlFileReader.ReadDataFromString<ConfigurationReadModel>(configTomlFile);

            var listOfModules = data.Module;

            listOfModules = listOfModules.Select((module, index) => new ModuleReadModel { Id = index, Config = module.Config, Name = module.Name, UUID = module.UUID }).ToList();

            return listOfModules;
        }
    }
}
