namespace Service.Controllers
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Business.Models.ConfigModels;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using ZTR.Framework.Service;

    [ApiController]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class DefaultValuesController : ControllerBase
    {
        private readonly IDefaultValueManager manager;

        public DefaultValuesController(IDefaultValueManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        /// <summary>
        /// Gets all modules.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetAll))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var result = await this.manager.GetAllModulesAsync().ConfigureAwait(false);

            return this.StatusCode(StatusCodes.Status200OK, result);
        }

        /// <summary>
        /// Gets the module and uuid information by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A <see cref="ModuleReadModel"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetByName))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByName([Required, FromQuery] string name)
        {
            var result = await this.manager.GetModuleByNameAsync(name).ConfigureAwait(false);
            return this.StatusCode(StatusCodes.Status200OK, result);
        }

        /// <summary>
        /// Gets the list of modules by names.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>A <see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetByNames))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByNames([Required, FromQuery] IEnumerable<string> names)
        {
            var result = await this.manager.GetModuleByNameAsync(names).ConfigureAwait(false);
            return this.StatusCode(StatusCodes.Status200OK, result);
        }

        /// <summary>
        /// Gets the list of modules by uuid.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetAllUuIds))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUuIds()
        {
            return this.StatusCode(StatusCodes.Status200OK, await this.manager.GetAllUuidsAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Gets the network information.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{NetworkReadModel}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetNetworkInformation))]
        [ProducesResponseType(typeof(IEnumerable<NetworkReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNetworkInformation()
        {
            return this.StatusCode(StatusCodes.Status200OK, await this.manager.GetNetworkInformationAsync().ConfigureAwait(false));
        }
    }
}
