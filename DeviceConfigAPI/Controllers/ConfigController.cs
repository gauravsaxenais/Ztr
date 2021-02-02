namespace Service.Controllers
{
    using Business.Parsers.Core.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Service;

    /// <summary>Config Controller - This service is responsible for generating the config toml.</summary>
    [System.ComponentModel.Description("Config Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class ConfigController : ApiControllerBase
    {
        private readonly IConfigManager _manager;
        private readonly ILogger<ConfigController> _logger;
        private readonly IConfigCreateFromManager _creator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigController"/> class.
        /// </summary>
        /// <param name="creator">The creator.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="logger">The logger.</param>
        public ConfigController(IConfigCreateFromManager creator, IConfigManager manager, ILogger<ConfigController> logger)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _manager = manager;
            _logger = logger;
            _creator = creator;
        }

        /// <summary>Creates the toml configuration.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost(nameof(CreateTomlConfig))]
        public async Task<IActionResult> CreateTomlConfig([FromBody] ConfigReadModel json)
        {
            var result = await _manager.CreateConfigAsync(json);
            return Ok(result);
        }
        //
        [HttpPost(nameof(CreateFromHtml))]
        [Consumes(SupportedContentTypes.MultipartFormData)]
        public async Task<IActionResult> CreateFromHtml(IFormFile htmlfile)
        {
            var toml = await _manager.CreateFromHtmlAsync("", "", htmlfile);
            var result = await _creator.GenerateConfigTomlModelAsync(toml);
            return Ok(result);
        }
    }
}
