namespace Service.Controllers
{
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
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
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml, SupportedContentTypes.MultipartFormData)]
    [QueryRoute]
    public class ConfigCreateFromController : ApiControllerBase
    {
        private readonly IConfigCreateFromManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigCreateFromController"/> class.
        /// </summary>
        /// <param name="manager">interface of the 'backend' _manager which does all the work.</param>
        public ConfigCreateFromController(IConfigCreateFromManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            _manager = manager;
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
        public async Task<IActionResult> GetConfigTomlValues([Required] IFormFile configTomlFile)
        {
            var result = await _manager.GenerateConfigTomlModelAsync(configTomlFile).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
