namespace Service.Controllers
{
    using Business.Parsers.Core.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Service;

    /// <summary>Config Controller - This service is responsible for generating the config toml.</summary>
    [System.ComponentModel.Description("Config Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml, SupportedContentTypes.MultipartFormData)]
    [QueryRoute]
    public class ConfigController : ApiControllerBase
    {
        private readonly IConfigManager _manager;
        private readonly ILogger<ConfigController> _logger;
        private readonly IConfigCreateFromManager _creator;
        private readonly IDefaultValueManager _defaultmanager;
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigController"/> class.
        /// </summary>
        /// <param name="creator">The creator.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="logger">The logger.</param>
        public ConfigController(IDefaultValueManager defaultmanager, IConfigCreateFromManager creator, IConfigManager manager, ILogger<ConfigController> logger)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _manager = manager;
            _logger = logger;
            _creator = creator;
            _defaultmanager = defaultmanager;
        }

        /// <summary>
        /// Creates the toml configuration.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        [HttpPost(nameof(CreateTomlConfig))]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTomlConfig([FromBody] ConfigReadModel json)
        {
            var result = await _manager.CreateConfigAsync(json);
            return Ok(result);
        }

        /// <summary>
        /// Creates from HTML.
        /// </summary>
        /// <param name="htmlFile">The html file.</param>
        /// <returns></returns>
        [HttpPost(nameof(CreateFromHtml))]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFromHtml([Required]
            [MaxFileSize(1 * 1024 * 1024)]
            [AllowedExtensions(new[] { ".html" })] IFormFile htmlFile)
        {
            var json = await _defaultmanager.GetDefaultValuesAllModulesAsync("1.0.38", "M7");
            var toml = await _manager.CreateFromHtmlAsync(htmlFile, json);
            var result = await _creator.GenerateConfigTomlModelWithoutGitAsync(toml);

            return Ok(result);
        }
    }
}
