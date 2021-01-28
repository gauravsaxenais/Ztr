namespace Service.Controllers
{
    using Business.Parsers.Core.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="logger">The logger.</param>
        public ConfigController(IConfigManager manager, ILogger<ConfigController> logger)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _manager = manager;
            _logger = logger;
        }

        /// <summary>Creates the toml configuration.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost(nameof(CreateTomlConfig))]
        public async Task<IActionResult> CreateTomlConfig([FromBody] ConfigReadModel json)
        {
            var prefix = nameof(ConfigController);
            _logger.LogInformation($"{prefix}: Creating config.toml.");

            var result = await _manager.CreateConfigAsync(json).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
