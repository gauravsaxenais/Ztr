namespace ZTR.Framework.Service.ExceptionLogger.ContentTypes
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Service;

    /// <summary>
    /// JsonExceptionContentType
    /// </summary>
    /// <seealso cref="ZTR.Framework.Service.ExceptionLogger.ContentTypes.AbstractExceptionContentType" />
    public class JsonExceptionContentType : AbstractExceptionContentType
    {
        /// <summary>
        /// Creates the exception response.
        /// </summary>
        /// <param name="problemDetails">The problem details.</param>
        /// <returns></returns>
        public override ExceptionResponse CreateExceptionResponse(ProblemDetails problemDetails)
        {
            return new ExceptionResponse(
                SupportedContentTypes.ProblemDetailsJson,
                JsonConvert.SerializeObject(problemDetails));
        }
    }
}
