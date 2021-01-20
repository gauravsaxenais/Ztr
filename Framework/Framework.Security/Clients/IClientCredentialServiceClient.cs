namespace ZTR.Framework.Security
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IClientCredentialServiceClient
    /// </summary>
    /// <seealso cref="ZTR.Framework.Security.IWebServiceClientBase" />
    public interface IClientCredentialServiceClient : IWebServiceClientBase
    {
        /// <summary>
        /// Sends the request asynchronous.
        /// </summary>
        /// <param name="serviceUri">The service URI.</param>
        /// <param name="httpVerb">The HTTP verb.</param>
        /// <param name="httpContentType">Type of the HTTP content.</param>
        /// <param name="content">The content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="apiTimeoutInMinutes">The API timeout in minutes.</param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendRequestAsync(Uri serviceUri, HttpVerb httpVerb, string httpContentType, StringContent content, CancellationToken cancellationToken, long? apiTimeoutInMinutes = null);

        /// <summary>
        /// Sends the request asynchronous.
        /// </summary>
        /// <param name="serviceUri">The service URI.</param>
        /// <param name="httpVerb">The HTTP verb.</param>
        /// <param name="httpContentType">Type of the HTTP content.</param>
        /// <param name="content">The content.</param>
        /// <param name="impersonateUser">The impersonate user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="apiTimeoutInMinutes">The API timeout in minutes.</param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendRequestAsync(Uri serviceUri, HttpVerb httpVerb, string httpContentType, StringContent content, IApplicationUser impersonateUser, CancellationToken cancellationToken, long? apiTimeoutInMinutes = null);
    }
}
