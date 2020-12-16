namespace Service.Controllers
{
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using ZTR.Framework.Business;

    /// <summary>Base controller implementing Base shared methods.</summary>
    [ApiController]
    [EnableCors(ApiConstants.ApiAllowAllOriginsPolicy)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public class ApiControllerBase : ControllerBase
    {
        public override OkObjectResult Ok([ActionResultObjectValue] object value)
        {
            return base.Ok(new ApiResponse { Success = true, Data = value } );
        }
    }
}
