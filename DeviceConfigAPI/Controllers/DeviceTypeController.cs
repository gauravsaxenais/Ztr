namespace Service.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using ZTR.Framework.Service;

    [ApiController]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class DeviceTypeController : ControllerBase
    {
        private readonly IDeviceTypeManager manager;

        public DeviceTypeController(IDeviceTypeManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        /// <summary>
        /// Gets all devices.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{string}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetAllDevices))]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDevices()
        {
            var result = await this.manager.GetAllDevicesAsync().ConfigureAwait(false);

            return this.StatusCode(StatusCodes.Status200OK, result);
        }

        /// <summary>
        /// Gets the current firmware versions from github repository.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A <see cref="IEnumerable{string}"/> representing the result of the operation.</returns>
        [HttpGet(nameof(GetAllFirmwareVersions))]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllFirmwareVersions()
        {
            var result = await this.manager.GetAllFirmwareVersionsAsync().ConfigureAwait(false);
            return this.StatusCode(StatusCodes.Status200OK, result);
        }
    }
}