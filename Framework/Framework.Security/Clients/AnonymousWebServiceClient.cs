namespace ZTR.Framework.Security
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using ZTR.Framework.Security.Clients;

    /// <summary>
    /// AnonymousWebServiceClient
    /// </summary>
    /// <seealso cref="WebServiceClientBase" />
    /// <seealso cref="IAnonymousWebServiceClient" />
    public class AnonymousWebServiceClient : WebServiceClientBase, IAnonymousWebServiceClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousWebServiceClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public AnonymousWebServiceClient(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

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
        public async Task<HttpResponseMessage> SendRequestAsync(Uri serviceUri, HttpVerb httpVerb, string httpContentType, StringContent content, CancellationToken cancellationToken, long? apiTimeoutInMinutes = null)
        {
            return await SendRequestAsync(serviceUri, httpVerb, httpContentType, content, apiTimeoutInMinutes, null, null, cancellationToken).ConfigureAwait(false);
        }
    }
}
