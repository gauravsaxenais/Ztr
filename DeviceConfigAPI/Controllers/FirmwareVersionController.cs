namespace Service.Controllers
{
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
    /// This service returns the device information.
    /// </summary>
    [System.ComponentModel.Description("Firmware version Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class FirmwareVersionController : ApiControllerBase
    {
        private readonly IFirmwareVersionManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionController"/> class.
        /// </summary>
        /// <param name="manager">The _manager.</param>
        public FirmwareVersionController(IFirmwareVersionManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            _manager = manager;
        }

        /// <summary>
        /// Gets all firmware versions.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        [HttpGet(nameof(GetAllFirmwareVersions))]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllFirmwareVersions([Required] string deviceType)
        {
            var result = await _manager.GetAllFirmwareVersionsAsync(deviceType).ConfigureAwait(false);
            return Ok(result);
        }
    }
}