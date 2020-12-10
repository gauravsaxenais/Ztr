namespace Service.Controllers
{
    using System.Threading.Tasks;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
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

        /// <summary>Gets all blocks.</summary>
        /// <returns>
        ///   list of blocks.
        /// </returns>
        [HttpGet(nameof(GetAllBlocks))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBlocks()
        {
            var result = await this.manager.ParseTomlFilesAsync().ConfigureAwait(false);

            return this.StatusCode(StatusCodes.Status200OK, result);
        }
    }
}
