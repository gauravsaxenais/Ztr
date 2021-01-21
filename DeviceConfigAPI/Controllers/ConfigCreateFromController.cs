namespace Service.Controllers
{
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Service;

    /// <summary>
    /// This service returns all the modules and their default values.
    /// If any module doesn't have any default values, then
    /// only the attributes are returned.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [System.ComponentModel.Description("Config Create From Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml, SupportedContentTypes.MultipartFormData)]
    [QueryRoute]
    public class ConfigCreateFromController : ApiControllerBase
    {
        private readonly IConfigCreateFromManager _manager;
        private readonly ILogger<ConfigCreateFromController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigCreateFromController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="logger">The logger.</param>
        public ConfigCreateFromController(IConfigCreateFromManager manager, ILogger<ConfigCreateFromController> logger)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _manager = manager;
            _logger = logger;
        }

        /// <summary>
        /// Gets the configuration toml values.
        /// </summary>
        /// <param name="configTomlFile">The configuration toml file.</param>
        /// <returns></returns>
        [HttpPost(nameof(GetConfigTomlValues))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConfigTomlValues(
            [Required]
            [MaxFileSize(1 * 1024 * 1024)]
            [AllowedExtensions(new[] { ".toml" })]
            IFormFile configTomlFile)
        {
            var prefix = nameof(ConfigCreateFromController);
            ApiResponse apiResponse;

            try
            {
                _logger.LogInformation($"{prefix}: Getting list of modules and blocks from config.toml file.");

                var result = await _manager.GenerateConfigTomlModelAsync(configTomlFile).ConfigureAwait(false);
                apiResponse = new ApiResponse(status: true, data: result);
                _logger.LogInformation($"{prefix}: Successfully retrieved list of modules and blocks from config.toml file.");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"{prefix}: Error occurred while getting list of modules and blocks from toml file.");
                apiResponse = new ApiResponse(false, exception.Message, ErrorType.BusinessError, exception);
            }

            return Ok(apiResponse);
        }
    }
}
