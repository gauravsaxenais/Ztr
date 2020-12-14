namespace Service.Controllers
{
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>Base controller implementing CorsPolicy.</summary>
    [ApiController]
    [EnableCors(ApiConstants.ApiAllowAllOriginsPolicy)]
    public class ApiControllerBase : ControllerBase
    {
    }
}
