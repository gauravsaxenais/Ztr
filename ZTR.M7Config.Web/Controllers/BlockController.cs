namespace Service.Controllers
{
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Swashbuckle.AspNetCore.Annotations;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using ZTR.Framework.Service;
    using ZTR.M7Config.Business.RequestHandlers.Interfaces;

    /// <summary>Block Controller - This service is responsible for getting arguments in network blocks.</summary>
    [System.ComponentModel.Description("Block Controller Service")]
    [SwaggerTag("This service returns all the blocks from Blocks repository.")]
    [Produces(SupportedContentTypes.Json)]
    [Consumes(SupportedContentTypes.Json)]
    [QueryRoute]
    public class BlockController : ApiControllerBase
    {
        private readonly IBlockManager _manager;
        private readonly ILogger<BlockController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="manager">The manager.</param>
        public BlockController(ILogger<BlockController> logger, IBlockManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _manager = manager;
            _logger = logger;
        }

        /// <summary>
        /// Displays a list of all blocks.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        [HttpGet(nameof(GetAllBlocks))]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBlocks([Required, FromQuery] string firmwareVersion, [Required, FromQuery] string deviceType)
        {
            var prefix = nameof(BlockController);
            _logger.LogInformation(
                $"{prefix}: Getting list of blocks for firmware version: {firmwareVersion} and device type: {deviceType}");
            var result = await _manager.GetBlocksAsync(firmwareVersion, deviceType).ConfigureAwait(false);
            _logger.LogInformation(
                $"{prefix}: Successfully retrieved list of blocks for firmware version: {firmwareVersion} and device type: {deviceType}");
            return Ok(result);
        }
    }
}
