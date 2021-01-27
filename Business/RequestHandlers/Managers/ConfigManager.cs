namespace Business.RequestHandlers.Managers
{
    using Business.Parsers.Core.Models;
    using EnsureThat;
    using Interfaces;
    using Parsers.Core.Converter;
    using System.Threading.Tasks;

    /// <summary>
    ///   <br />
    /// </summary>
    public class ConfigManager : IConfigManager
    {
        private readonly ConverterService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigManager"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public ConfigManager(ConverterService service)
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
        /// Read from url asynchronous.
        /// </summary>      
        public async Task<string> CreateFromHtmlAsync()
        { 
            return await _service.CreateFromHtmlAsync();
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
