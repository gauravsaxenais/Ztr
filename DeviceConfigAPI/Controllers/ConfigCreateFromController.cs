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
        /// Gets default values for all the modules.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        /// <param name="configTomlString">configtoml as string.</param>
        [HttpGet(nameof(GetAllDefaultValues))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDefaultValues([Required, FromQuery] string configTomlString)
        {
            var result = await this.manager.GenerateConfigTomlModelAsync(configTomlString).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
