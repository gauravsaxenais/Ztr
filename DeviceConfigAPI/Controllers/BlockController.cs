namespace Service.Controllers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using ZTR.Framework.Service;

    [ApiController]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class BlockController : ControllerBase
    {
        private readonly IBlockManager manager;

        public BlockController(IBlockManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        [HttpGet(nameof(GetAllBlocks))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBlocks([Required, FromQuery] string firmwareVersion, string deviceType)
        {
            var result = await this.manager.ParseTomlFilesAsync(firmwareVersion, deviceType, "blocks").ConfigureAwait(false);
            if (string.IsNullOrEmpty(result))
            {
                return this.StatusCode(StatusCodes.Status200OK, result);
            }

            var json = JObject.Parse(result);
            return this.StatusCode(StatusCodes.Status200OK, json);
        }
    }
}
