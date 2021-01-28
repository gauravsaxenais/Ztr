namespace Service.Controllers
{
    using Business.Common.Models;
    using Business.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using ZTR.Framework.Service;

    /// <summary>
    /// This controller returns the list of compatible firmware versions.
    /// It takes input a firmware version and returns an array of firmware versions.
    /// </summary>
    /// <seealso cref="ApiControllerBase" />
    [System.ComponentModel.Description("Compatible Firmware Version Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml, SupportedContentTypes.MultipartFormData)]
    [QueryRoute]
    public class CompatibleFirmwareVersionController : ApiControllerBase
    {
        private readonly ICompatibleFirmwareVersionManager _manager;
        private readonly ILogger<CompatibleFirmwareVersionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibleFirmwareVersionController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="logger">The logger.</param>
        public CompatibleFirmwareVersionController(ICompatibleFirmwareVersionManager manager, ILogger<CompatibleFirmwareVersionController> logger)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            _manager = manager;
            _logger = logger;
        }

        /// <summary>
        /// Gets the compatible firmware versions.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        [HttpPost(nameof(GetCompatibleFirmwareVersions))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompatibleFirmwareVersions([Required, FromBody] CompatibleFirmwareVersionReadModel module)
        {
            var prefix = nameof(CompatibleFirmwareVersionController);
            _logger.LogInformation($"{prefix}: Getting list of compatible firmware versions based on a firmware version.");

            var result = await _manager.GetCompatibleFirmwareVersionsAsync(module).ConfigureAwait(false);
            
            return Ok(result);
        }
    }
}
