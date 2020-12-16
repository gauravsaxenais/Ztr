namespace ZTR.Framework.Service.ExceptionLogger
{
    using System;
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
        private const string ProblemTitle = "An unexpected error occurred!";
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

        private static async Task WriteResponse(HttpContext context, ExceptionResponse exceptionResponse, ProblemDetails problemDetails)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = exceptionResponse == null
                ? SupportedContentTypes.TextPlain
                : exceptionResponse.ResponseContentType;
            await context.Response
                .WriteAsync(exceptionResponse == null
                    ? problemDetails.ToFormattedString()
                    : exceptionResponse.ResponseText).ConfigureAwait(false);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ExceptionResponse exceptionResponse = default;
            _logger.LogCritical(exception, nameof(ExceptionMiddleware));

            ProblemDetails problemDetails = CreateProblemDetailsObject(exception);
            if (context.Request.Headers.TryGetValue("Accept", out var acceptContentTypes))
            {
                if (acceptContentTypes.Contains(SupportedContentTypes.Json))
                {
                    exceptionResponse = new JsonExceptionContentType().CreateExceptionResponse(problemDetails);
                }
            }

            await WriteResponse(context, exceptionResponse, problemDetails).ConfigureAwait(false);
        }

        private ProblemDetails CreateProblemDetailsObject(Exception exception)
        {
            string errorDetail;
            if (_hostingEnvironment.IsDevelopment())
            {
                var error = new ErrorMessage<ErrorType>(ErrorType.ServerError, exception);
                errorDetail = error.ToString();
            }
            else
            {
                errorDetail = "An error occurred please contact administrator";
            }

            return new ProblemDetails
            {
                Title = ProblemTitle,
                Status = StatusCodes.Status400BadRequest,
                Detail = errorDetail,
                Instance = $"urn:MyOrganization:error:{Guid.NewGuid()}"
            };
        }
    }
}
