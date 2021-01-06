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
    /// This class return the list of modules and their
    /// name and uuid information.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [System.ComponentModel.Description("Module Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class ModuleController : ApiControllerBase
    {
        private readonly IModuleManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleController"/> class.
        /// </summary>
        /// <param name="manager">The _manager.</param>
        public ModuleController(IModuleManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            _manager = manager;
        }

        /// <summary>
        /// Gets all modules.
        /// </summary>
        /// <param name="firmwareVersion">firmware version or tag in github.</param>
        /// <param name="deviceType">device type e.g. M3, M7.</param>
        /// <returns><see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetAllModules))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllModules([Required, FromQuery] string firmwareVersion, [Required, FromQuery] string deviceType)
        {
            var result = await _manager.GetAllModulesAsync(firmwareVersion, deviceType).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
