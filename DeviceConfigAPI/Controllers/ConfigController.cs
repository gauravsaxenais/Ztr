namespace Service.Controllers
{
    using Business.Parsers.Core.Models;
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
        private readonly IConfigGeneratorManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigController"/> class.
        /// </summary>
        /// <param name="manager">The _manager.</param>
        public ConfigController(IConfigGeneratorManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            _manager = manager;
        }

        /// <summary>Creates the toml configuration.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost(nameof(CreateTomlConfig))]
        public async Task<IActionResult> CreateTomlConfig([FromBody] ConfigReadModel json)
        {
            var result = await _manager.CreateConfigAsync(json).ConfigureAwait(false);
            return Ok(result);
        }
    }
}
