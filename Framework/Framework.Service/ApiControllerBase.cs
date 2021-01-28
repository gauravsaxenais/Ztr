namespace ZTR.Framework.Service
{
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;

    /// <summary>Base controller implementing Base shared methods.</summary>
    [ApiController]
    [EnableCors(ApiConstants.ApiAllowAllOriginsPolicy)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Creates an <see cref="OkObjectResult" /> object that produces an <see cref="StatusCodes.Status200OK" /> response.
        /// </summary>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>
        /// The created <see cref="OkObjectResult" /> for the response.
        /// </returns>
        public override OkObjectResult Ok([ActionResultObjectValue] object value)
        {
            return base.Ok(new ApiResponse { Success = true, Data = value });
        }
    }
}
