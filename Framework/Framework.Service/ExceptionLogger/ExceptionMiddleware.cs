namespace ZTR.Framework.Service.ExceptionLogger
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using ContentTypes;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Service;
    using ZTR.Framework.Business;

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate requestDelegate, IHostEnvironment hostingEnvironment, ILogger<ExceptionMiddleware> logger)
        {
            _requestDelegate = requestDelegate;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

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

        private static async Task WriteResponse(HttpContext context, ExceptionResponse exceptionResponse, ApiResponse problemDetails)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response
                .WriteAsync(exceptionResponse == null
                    ? problemDetails.ToString()
                    : exceptionResponse.ResponseText).ConfigureAwait(false);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ExceptionResponse exceptionResponse = default;
            _logger.LogCritical(exception, nameof(ExceptionMiddleware));
            var error = new ErrorMessage<ErrorType>(ErrorType.ServerError, exception);
            var response = new ApiResponse { Success = false, Error = error };

            await WriteResponse(context, exceptionResponse, response);
        }


    }
}
