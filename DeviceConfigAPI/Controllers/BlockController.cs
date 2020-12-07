namespace Service.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using ZTR.Framework.Service;

    /// <summary>Block Controller - This service is responsible for getting arguments in network blocks.</summary>
    [ApiController]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class BlockController : ControllerBase
    {
        private readonly IBlockManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public BlockController(IBlockManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        /// <summary>
        /// Gets all blocks.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns>returns the block information and their arguments.</returns>
        [HttpGet(nameof(GetAllBlocks))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBlocks([Required, FromQuery] string firmwareVersion, [Required, FromQuery] string deviceType)
        {
            var result = await this.manager.ParseTomlFilesAsync(firmwareVersion, deviceType, "blocks").ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(result))
            {
                return this.StatusCode(StatusCodes.Status200OK, result);
            }

            var json = JObject.Parse(result);
            return this.StatusCode(StatusCodes.Status200OK, json);
        }
    }
}
