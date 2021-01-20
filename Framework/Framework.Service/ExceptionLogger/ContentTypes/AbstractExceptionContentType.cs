namespace ZTR.Framework.Service.ExceptionLogger.ContentTypes
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// AbstractExceptionContentType
    /// </summary>
    public abstract class AbstractExceptionContentType
    {
        /// <summary>
        /// Creates the exception response.
        /// </summary>
        /// <param name="problemDetails">The problem details.</param>
        /// <returns></returns>
        public abstract ExceptionResponse CreateExceptionResponse(ProblemDetails problemDetails);
    }
}
