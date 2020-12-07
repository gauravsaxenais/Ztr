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
    using ZTR.Framework.Service;

    /// <summary>
    /// This service returns all the modules and their default values.
    /// If any module doesnt have any default values, then
    /// only the attributes are returned.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ApiController]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class DefaultValuesController : ControllerBase
    {
        private readonly IDefaultValueManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValuesController"/> class.
        /// </summary>
        /// <param name="manager">interface of the 'backend' manager which does all the work.</param>
        public DefaultValuesController(IDefaultValueManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        /// <summary>
        /// Gets default values for all the modules.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        /// <param name="firmwareVersion">firmware version.</param>
        /// <param name="deviceType">device type.</param>
        [HttpGet(nameof(GetAllDefaultValues))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDefaultValues([Required, FromQuery] string firmwareVersion, [Required, FromQuery] string deviceType)
        {
            var result = await this.manager.GetDefaultValuesAllModulesAsync(firmwareVersion, deviceType).ConfigureAwait(false);

            return this.StatusCode(StatusCodes.Status200OK, result);
        }
    }
}
