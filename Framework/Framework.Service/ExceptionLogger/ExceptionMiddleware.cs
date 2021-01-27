namespace ZTR.Framework.Service.ExceptionLogger
{
    using System;
    using System.Threading.Tasks;
    using Business;
    using Business.Content;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ExceptionMiddleware
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ILogger<ExceptionMiddleware> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="requestDelegate">The request delegate.</param>
        /// <param name="hostingEnvironment">The hosting environment.</param>
        /// <param name="logger">The logger.</param>
        public ExceptionMiddleware(RequestDelegate requestDelegate, IHostEnvironment hostingEnvironment, ILogger<ExceptionMiddleware> logger)
        {
            _requestDelegate = requestDelegate;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the asynchronous.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                if (httpContext != null)
                {
                    await (_requestDelegate?.Invoke(httpContext)).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex).ConfigureAwait(false);
            }
        }

        private static async Task WriteResponse(HttpContext context, ApiResponse apiResponse)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = SupportedContentTypes.Json;

            await context.Response
                .WriteAsync(apiResponse.ToString()).ConfigureAwait(false);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogCritical(exception, nameof(ExceptionMiddleware));

            ErrorMessage<ErrorType> error;
            if (_hostingEnvironment.IsDevelopment())
            {
                error = new ErrorMessage<ErrorType>(ErrorType.ServerError, exception);
            }
            else
            {
                error = new ErrorMessage<ErrorType>(ErrorType.ServerError, Resource.ExceptionMessage);
            }

            var response = new ApiResponse { Success = false, Error = error };
            await WriteResponse(context, response).ConfigureAwait(false);
        }
    }
}
