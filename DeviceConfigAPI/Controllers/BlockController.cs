namespace Service.Controllers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using ZTR.Framework.Service;

    /// <summary>Block Controller - This service is responsible for getting arguments in network blocks.</summary>
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class BlockController : ApiControllerBase
    {
        private readonly IBlockManager _manager;
        private readonly ILogger<BlockController> _logger;
        private const string Prefix = nameof(BlockController);

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

        /// <summary>Gets all blocks.</summary>
        /// <returns>
        ///   list of blocks.
        /// </returns>
        [HttpGet(nameof(GetAllBlocks))]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBlocks()
        {
            var prefix = nameof(BlockController);
            _logger.LogInformation($"{prefix}: Getting list of blocks.");
            var result = await _manager.GetBlocksAsync().ConfigureAwait(false);

            var apiResponse = new ApiResponse(status: true, data: result);
            return Ok(apiResponse);
        }
    }
}
