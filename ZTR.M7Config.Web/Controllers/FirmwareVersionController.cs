namespace Service.Controllers
{
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Swashbuckle.AspNetCore.Annotations;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using System.Web;
    using ZTR.Framework.Service;
    using ZTR.M7Config.Business.RequestHandlers.Interfaces;

    /// <summary>
    /// This service returns the device information.
    /// </summary>
    [System.ComponentModel.Description("Firmware version Controller Service")]
    [Produces(SupportedContentTypes.Json)]
    [Consumes(SupportedContentTypes.Json)]
    [QueryRoute]
    [SwaggerTag("This service returns all the firmware versions for a particular device e.g. M7.")]
    public class FirmwareVersionController : ApiControllerBase
    {
        private readonly IFirmwareVersionManager _manager;
        private readonly ILogger<FirmwareVersionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="logger">The logger.</param>
        public FirmwareVersionController(IFirmwareVersionManager manager, ILogger<FirmwareVersionController> logger)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            _manager = manager;
            _logger = logger;
        }

        /// <summary>
        /// Displays a list of all firmware versions for a particular device.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        [HttpGet(nameof(GetAllFirmwareVersions))]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllFirmwareVersions([Required, FromQuery] string deviceType)
        {
            var prefix = nameof(FirmwareVersionController);

            _logger.LogInformation($"{prefix}: Getting list of all firmware versions for {deviceType}");
            deviceType = HttpUtility.UrlDecode(deviceType);
            var result = await _manager.GetAllFirmwareVersionsAsync(deviceType).ConfigureAwait(false);
            _logger.LogInformation($"{prefix}: Successfully retrieved list of all firmware versions");

            return Ok(result);
        }
    }
}