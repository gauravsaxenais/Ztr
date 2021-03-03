namespace Business.RequestHandlers.Managers
{
    using Business.Common.Models;
    using Business.Parsers.Core.Models;
    using EnsureThat;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Parsers.Core.Converter;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// ConfigManager service.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IConfigManager" />
    public class ConfigManager : Manager, IConfigManager
    {
        private readonly ConverterService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="service">The service.</param>
        public ConfigManager(ILogger<DefaultValueManager> logger, ConverterService service) : base(logger)
        {
            _service = service;
        }

        /// <summary>
        /// Creates the configuration asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task<string> CreateConfigAsync(ConfigReadModel model)
        {
            EnsureArg.IsNotNull(model);
            EnsureArg.IsNotEmptyOrWhiteSpace(model.Block);
            EnsureArg.IsNotEmptyOrWhiteSpace(model.Module);

            return await _service.CreateConfigTomlAsync(model);
        }

        /// <summary>
        /// Creates from HTML asynchronous.
        /// </summary>
        /// <param name="htmlFile">The HTML file.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public async Task<string> CreateFromHtmlAsync(IFormFile htmlFile, IEnumerable<ModuleReadModel> values)
        {
            var module = new { module = values };
            var model = new ConfigReadModel { Module = JsonConvert.SerializeObject(module), Block = "{}", Version = "{}" };
            var Toml =
                 new ConfigTOML
                 {
                     BaseToml = await _service.CreateConfigTomlAsync(model, true),
                     ViewToml = await _service.CreateConfigTomlAsync(model, true)
                 };

            var html = FileReaderExtensions.ReadAsString(htmlFile);            

            return await _service.CreateFromHtmlAsync(html, Toml);
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
