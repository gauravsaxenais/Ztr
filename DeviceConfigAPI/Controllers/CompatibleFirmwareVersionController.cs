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
        private readonly ICompatibleFirmwareVersionManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibleFirmwareVersionController"/> class.
        /// </summary>
        /// <param name="manager">interface of the 'backend' manager which does all the work.</param>
        public CompatibleFirmwareVersionController(ICompatibleFirmwareVersionManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
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
            var result = await manager.GetCompatibleFirmwareVersionsAsync(module).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
