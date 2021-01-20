namespace ZTR.Framework.Business
{
    using System.Collections.Generic;

    /// <summary>
    /// IApi Exception
    /// </summary>
    public interface IApiException
    {
        /// <summary>
        /// Gets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        int StatusCode { get; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        string Response { get; }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }
    }
}
