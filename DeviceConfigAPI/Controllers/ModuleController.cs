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
    /// This class return the list of modules and their
    /// name and uuid information.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [System.ComponentModel.Description("Module Controller Service")]
    [ApiController]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public ModuleController(IModuleManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        /// <summary>
        /// Gets all modules.
        /// </summary>
        /// <param name="firmwareVersion">firmware version or tag in github.</param>
        /// <param name="deviceType">device type e.g. M3, M7.</param>
        /// <returns><see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetAllModules))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllModules([Required, FromQuery] string firmwareVersion, [Required, FromQuery] string deviceType)
        {
            var result = await this.manager.GetAllModulesAsync(firmwareVersion, deviceType).ConfigureAwait(false);

            return this.StatusCode(StatusCodes.Status200OK, result);
        }

        /// <summary>
        /// Get the module information by name.
        /// </summary>
        /// <param name="name">name of the module.</param>
        /// <param name="firmwareVersion">firmware version.</param>
        /// <param name="deviceType">device type.</param>
        /// <returns>A <see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetModuleByName))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModuleByName([Required, FromQuery] string name, [Required, FromQuery] string firmwareVersion, string deviceType)
        {
            var result = await this.manager.GetModuleByNameAsync(name, firmwareVersion, deviceType).ConfigureAwait(false);
            return this.StatusCode(StatusCodes.Status200OK, result);
        }

        /// <summary>
        /// Gets the list of modules by names.
        /// </summary>
        /// <param name="firmwareVersion">firmware version.</param>
        /// <param name="deviceType">evice type.</param>
        /// <param name="names">names of the modules.</param>
        /// <returns>A <see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetModuleByNames))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModuleByNames([Required, FromQuery] string firmwareVersion, [Required, FromQuery] string deviceType, IEnumerable<string> names)
        {
            var result = await this.manager.GetModelByNameAsync(firmwareVersion, deviceType, names).ConfigureAwait(false);
            return this.StatusCode(StatusCodes.Status200OK, result);
        }
    }
}
