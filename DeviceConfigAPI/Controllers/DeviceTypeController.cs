namespace Service.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Business.Parsers.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using ZTR.Framework.Service;

    /// <summary>
    /// This service returns the device information.
    /// </summary>
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class DeviceTypeController : ApiControllerBase
    {
        private readonly IDeviceTypeManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTypeController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public DeviceTypeController(IDeviceTypeManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        /// <summary>
        /// Gets all devices.
        /// </summary>
        /// <returns>status code and the output.</returns>
        [HttpGet(nameof(GetAllDevices))]
        public async Task<IActionResult> GetAllDevices()
        {
            var result = await this.manager.GetAllDevicesAsync().ConfigureAwait(false);

            return Ok(result);
        }

        /// <summary>
        /// Gets the current firmware versions from github repository.
        /// </summary>
        /// <returns>status code representing the result of the operation and the result.</returns>
        [HttpGet(nameof(GetAllFirmwareVersions))]
        public async Task<IActionResult> GetAllFirmwareVersions()
        {
            var result = await this.manager.GetAllFirmwareVersionsAsync().ConfigureAwait(false);
            return Ok(result);
        }
    }
}