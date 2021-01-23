namespace Business.RequestHandlers.Managers
{
    using Business.Common.Models;
    using EnsureThat;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Models;
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
        private readonly IDefaultValueManager _defaultValueManager;
        private readonly IBlockManager _blockManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigCreateFromManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="defaultValueManager">The default value manager.</param>
        /// <param name="blockManager">The block manager.</param>
        public ConfigCreateFromManager(ILogger<DefaultValueManager> logger, IDefaultValueManager defaultValueManager, IBlockManager blockManager) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(defaultValueManager, nameof(defaultValueManager));
            EnsureArg.IsNotNull(blockManager, nameof(blockManager));

            _blockManager = blockManager;
            _defaultValueManager = defaultValueManager;
            _logger = logger;
        }

        /// <summary>
        /// Returns the list of all modules and blocks from config.toml.
        /// </summary>
        /// <param name="configTomlFile">config.toml as string.</param>
        /// <returns></returns>
        public async Task<object> GenerateConfigTomlModelAsync(IFormFile configTomlFile)
        {
            EnsureArg.IsNotNull(configTomlFile);
            var prefix = nameof(ConfigCreateFromManager);

            _logger.LogInformation($"{prefix}: methodName: {nameof(GenerateConfigTomlModelAsync)} Getting list of modules and blocks from config.toml file.");

            var configTomlFileContent = ReadAsString(configTomlFile);

            var (modules, blocks) = await GetModulesAndBlocksAsync(configTomlFileContent);

            return new { modules, blocks };
        }

        private async Task<(IEnumerable<ModuleReadModel>, IEnumerable<BlockJsonModel>)> GetModulesAndBlocksAsync(string configTomlFileContent)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(configTomlFileContent);

            // get list of all modules.
            var modules = GetListOfModules(configTomlFileContent).ToList();

            await _defaultValueManager.MergeValuesWithModulesAsync(configTomlFileContent, modules).ConfigureAwait(false);

            var blocks = await _blockManager.GetListOfBlocksAsync().ConfigureAwait(false);
            
            return (modules, blocks);
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
