namespace Service.Controllers
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using ZTR.Framework.Business;
    using ZTR.Framework.Service;

    /// <summary>
    /// This service returns all the modules and their default values.
    /// If any module doesnt have any default values, then
    /// only the attributes are returned.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [System.ComponentModel.Description("Config Create From Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class ConfigCreateFromController : ApiControllerBase
    {
        private readonly IConfigCreateFromManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigCreateFromController"/> class.
        /// </summary>
        /// <param name="manager">interface of the 'backend' manager which does all the work.</param>
        public ConfigCreateFromController(IConfigCreateFromManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        /// <summary>
        /// Gets the configuration toml values.
        /// </summary>
        /// <param name="configTomlFile">The configuration toml file.</param>
        /// <returns></returns>
        [HttpGet(nameof(GetConfigTomlValues))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConfigTomlValues([Required, FromForm(Name = "config.toml")] IFormFile configTomlFile)
        {
            var result = await this.manager.GenerateConfigTomlModelAsync(configTomlFile).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
