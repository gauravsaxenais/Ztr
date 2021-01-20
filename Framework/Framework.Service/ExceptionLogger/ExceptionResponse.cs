namespace ZTR.Framework.Service.ExceptionLogger
{
    /// <summary>
    /// ExceptionResponse
    /// </summary>
    public class ExceptionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionResponse"/> class.
        /// </summary>
        /// <param name="responseContentType">Type of the response content.</param>
        /// <param name="responseText">The response text.</param>
        public ExceptionResponse(string responseContentType, string responseText)
        {
            ResponseContentType = responseContentType;
            ResponseText = responseText;
        }

        /// <summary>
        /// Gets the type of the response content.
        /// </summary>
        /// <value>
        /// The type of the response content.
        /// </value>
        public string ResponseContentType { get; }

        /// <summary>
        /// Gets the response text.
        /// </summary>
        /// <value>
        /// The response text.
        /// </value>
        public string ResponseText { get; }
    }
}
