namespace Service.Controllers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using ZTR.Framework.Service;

    /// <summary>Block Controller - This service is responsible for getting arguments in network blocks.</summary>
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class BlockController : ApiControllerBase
    {
        private readonly IBlockManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public BlockController(IBlockManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            _manager = manager;
        }

        /// <summary>Gets all blocks.</summary>
        /// <returns>
        ///   list of blocks.
        /// </returns>
        [HttpGet(nameof(GetAllBlocks))]
        public async Task<IActionResult> GetAllBlocks()
        {
            var result = await this._manager.GetBlocksAsObjectAsync().ConfigureAwait(false);

            return Ok(result);
        }
    }
}
