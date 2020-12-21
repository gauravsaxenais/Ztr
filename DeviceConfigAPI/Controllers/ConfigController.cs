namespace Service.Controllers
{
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using ZTR.Framework.Service;

    /// <summary>Config Controller - This service is responsible for generating the config toml.</summary>
    [System.ComponentModel.Description("Config Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class ConfigController : ApiControllerBase
    {
        private readonly IConfigGeneratorManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public ConfigController(IConfigGeneratorManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        /// <summary>Creates the toml configuration.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost(nameof(CreateTomlConfig))]
        public async Task<IActionResult> CreateTomlConfig([FromBody] ConfigReadModel json)
        {
            var result = await manager.CreateConfigAsync(json).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet(nameof(ConfigRule))]
        public async Task<IActionResult> ConfigRule(string properties)
        {
            var result = await manager.UpdateTomlConfig(properties).ConfigureAwait(false);
            return Ok(result);
        }
    }
}
