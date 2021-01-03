namespace Business.RequestHandlers.Managers
{
    using Business.Parsers.Core.Models;
    using Business.Parsers.TomlParser.Core.Converter;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using System.Threading.Tasks;

    /// <summary>
    ///   <br />
    /// </summary>
    public class ConfigGeneratorManager : IConfigGeneratorManager
    {
        private readonly ConverterService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigGeneratorManager"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public ConfigGeneratorManager(ConverterService service)
        {
            _service = service;
        }
        /// <summary>
        /// Creates the configuration asynchronous.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<string> CreateConfigAsync(ConfigReadModel model)
        {
            EnsureArg.IsNotNull(model);
            EnsureArg.IsNotEmptyOrWhiteSpace(model.Block);
            EnsureArg.IsNotEmptyOrWhiteSpace(model.Module);

            return await _service.CreateConfigTomlAsync(model);
        }

        /// <summary>
        /// Updates the toml configuration.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public async Task<bool> UpdateTomlConfig(string properties)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(properties);
            return await _service.UpdateConfig(properties);
        }
    }
}
