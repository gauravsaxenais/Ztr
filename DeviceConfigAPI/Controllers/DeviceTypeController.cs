namespace Service.Controllers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Service;

    /// <summary>
    /// This service returns the device information.
    /// </summary>
    [System.ComponentModel.Description("Device Type Controller Service")]
    [Produces(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [Consumes(SupportedContentTypes.Json, SupportedContentTypes.Xml)]
    [QueryRoute]
    public class DeviceTypeController : ApiControllerBase
    {
        private readonly IDeviceTypeManager _manager;
        private readonly ILogger<DeviceTypeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTypeController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="logger">The logger.</param>
        public DeviceTypeController(IDeviceTypeManager manager, ILogger<DeviceTypeController> logger)
        {
            EnsureArg.IsNotNull(manager, nameof(manager));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _manager = manager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all devices.
        /// </summary>
        /// <returns>all devices.</returns>
        [HttpGet(nameof(GetAllDevices))]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDevices()
        {
            ApiResponse apiResponse;
            var prefix = nameof(DeviceTypeController);

            try
            {
                _logger.LogInformation($"{prefix} method name: {nameof(GetAllDevices)}: Getting list of all devices.");

                var result = await _manager.GetAllDevicesAsync().ConfigureAwait(false);

                apiResponse = new ApiResponse(status: true, data: result);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"{prefix}: method name: {nameof(GetAllDevices)} Error occurred while getting list of all devices.");
                apiResponse = new ApiResponse(ErrorType.BusinessError, exception);
            }

            return Ok(apiResponse);
        }
    }
}