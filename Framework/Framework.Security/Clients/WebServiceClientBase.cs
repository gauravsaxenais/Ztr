namespace ZTR.Framework.Security
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;

    /// <summary>
    /// WebServiceClientBase
    /// </summary>
    /// <seealso cref="ZTR.Framework.Security.IWebServiceClientBase" />
    public abstract class WebServiceClientBase : IWebServiceClientBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientBase"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public WebServiceClientBase(IHttpClientFactory httpClientFactory)
        {
            EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));

            HttpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Gets or sets the request handler.
        /// </summary>
        /// <value>
        /// The request handler.
        /// </value>
        public Func<HttpRequestMessage, Task> RequestHandler { get; set; }

        /// <summary>
        /// Gets or sets the client handler.
        /// </summary>
        /// <value>
        /// The client handler.
        /// </value>
        public Func<HttpClient, Task> ClientHandler { get; set; }

        /// <summary>
        /// Gets the HTTP client factory.
        /// </summary>
        /// <value>
        /// The HTTP client factory.
        /// </value>
        protected IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Sends the request asynchronous.
        /// </summary>
        /// <param name="serviceUri">The service URI.</param>
        /// <param name="httpVerb">The HTTP verb.</param>
        /// <param name="httpContentType">Type of the HTTP content.</param>
        /// <param name="content">The content.</param>
        /// <param name="apiTimeoutInMinutes">The API timeout in minutes.</param>
        /// <param name="configRequestMessage">The configuration request message.</param>
        /// <param name="configClient">The configuration client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> SendRequestAsync(Uri serviceUri, HttpVerb httpVerb, string httpContentType, StringContent content, long? apiTimeoutInMinutes, Func<HttpRequestMessage, Task> configRequestMessage, Func<HttpClient, Task> configClient, CancellationToken cancellationToken)
        {
            using (var request = await GetRequestMessage(cancellationToken).ConfigureAwait(false))
            {
                request.ConfigHttpRequestMessage(serviceUri, httpVerb, httpContentType, content);

                if (configRequestMessage != null)
                {
                    await configRequestMessage(request).ConfigureAwait(false);
                }

                if (RequestHandler != null)
                {
                    await RequestHandler(request).ConfigureAwait(false);
                }

                using (HttpClient client = HttpClientFactory.CreateClient(serviceUri.AbsoluteUri))
                {
                    client.ConfigHttpClient(apiTimeoutInMinutes, ServiceClientConstants.UserAgentHeaders);

                    if (configClient != null)
                    {
                        await configClient(client).ConfigureAwait(false);
                    }

                    if (ClientHandler != null)
                    {
                        await ClientHandler(client).ConfigureAwait(false);
                    }

                    return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Gets the request message.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected virtual Task<HttpRequestMessage> GetRequestMessage(CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpRequestMessage());
        }
    }
}
