namespace Service.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using ZTR.Framework.Service;

    /// <summary>Config Controller - This service is responsible for generating the config toml.</summary>
    [ApiController]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class ConfigController : ControllerBase
    {
        private readonly IConfigGeneratorManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public ConfigController(IConfigGeneratorManager manager)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));

            this.manager = manager;
        }

        [HttpPost(nameof(CreateTomlConfig))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTomlConfig()
        {
            string jsonContent = System.IO.File.ReadAllText(@"C:\Users\admin.DESKTOP-G7578TS\source\ZTR\DeviceConfigAPI\bin\Debug\netcoreapp3.1\BlockConfig\config\config.json");
            var result = await this.manager.CreateConfigAsync(jsonContent).ConfigureAwait(false);
            if (string.IsNullOrEmpty(result))
            {
                return this.StatusCode(StatusCodes.Status200OK, result);
            }

            var json = result;
            return this.StatusCode(StatusCodes.Status200OK, json);
        }
    }
}
