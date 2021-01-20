namespace ZTR.Framework.Service
{
    using System.Text;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// ProblemDetailsExtensions
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        /// <summary>
        /// Converts to formattedstring.
        /// </summary>
        /// <param name="problemDetails">The problem details.</param>
        /// <returns></returns>
        public static string ToFormattedString(this ProblemDetails problemDetails)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Type: {problemDetails.Type}");
            stringBuilder.AppendLine($"Title: {problemDetails.Title}");
            stringBuilder.AppendLine($"Status: {problemDetails.Status}");
            stringBuilder.AppendLine($"Detail: {problemDetails.Detail}");
            stringBuilder.AppendLine($"Instance: {problemDetails.Instance}");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts to formatted string.
        /// </summary>
        /// <param name="validationProblemDetails">The validation problem details.</param>
        /// <returns></returns>
        public static string ToFormattedString(this ValidationProblemDetails validationProblemDetails)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Type: {validationProblemDetails.Type}");
            stringBuilder.AppendLine($"Title: {validationProblemDetails.Title}");
            stringBuilder.AppendLine($"Status: {validationProblemDetails.Status}");
            stringBuilder.AppendLine($"Detail: {validationProblemDetails.Detail}");
            stringBuilder.AppendLine($"Instance: {validationProblemDetails.Instance}");
            stringBuilder.AppendLine($"Instance: {validationProblemDetails.Errors}");

            return stringBuilder.ToString();
        }
    }
}
