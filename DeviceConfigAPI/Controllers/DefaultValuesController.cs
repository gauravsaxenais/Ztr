namespace Service.Controllers
{
    using Business.Common.Models;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Service;

    /// <summary>
    /// This service returns all the modules and their default values.
    /// If any module doesn't have any default values, then
    /// only the attributes are returned.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [System.ComponentModel.Description("Default Values Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class DefaultValuesController : ApiControllerBase
    {
        private readonly IDefaultValueManager _manager;
        private readonly ILogger<DefaultValuesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValuesController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="logger">The logger.</param>
        public DefaultValuesController(IDefaultValueManager manager, ILogger<DefaultValuesController> logger)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _manager = manager;
            _logger = logger;
        }

        /// <summary>
        /// Gets default values for all the modules.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{ModuleReadModel}"/> representing the result of the operation.</returns>
        /// <param name="firmwareVersion">firmware version.</param>
        /// <param name="deviceType">device type.</param>
        [HttpGet(nameof(GetAllDefaultValues))]
        [ProducesResponseType(typeof(IEnumerable<ModuleReadModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDefaultValues([Required, FromQuery] string firmwareVersion, [Required, FromQuery] string deviceType)
        {
            var prefix = nameof(DefaultValuesController);
            _logger.LogInformation($"{prefix}: Getting default values for {firmwareVersion} and {deviceType}.");

            var result = await _manager.GetDefaultValuesAllModulesAsync(firmwareVersion, deviceType).ConfigureAwait(false);
            var apiResponse = new ApiResponse(status: true, data: result);

            return Ok(apiResponse);
        }
    }
}
