namespace ZTR.Framework.Security
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// HttpRequestMessageExtension
    /// </summary>
    public static class HttpRequestMessageExtension
    {
        /// <summary>
        /// Configurations the HTTP request message.
        /// </summary>
        /// <param name="httpRequestMessage">The HTTP request message.</param>
        /// <param name="serviceUri">The service URI.</param>
        /// <param name="httpVerb">The HTTP verb.</param>
        /// <param name="httpContentType">Type of the HTTP content.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static HttpRequestMessage ConfigHttpRequestMessage(this HttpRequestMessage httpRequestMessage, Uri serviceUri, HttpVerb httpVerb, string httpContentType, StringContent content)
        {
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(httpContentType);
            httpRequestMessage.Content = content;
            httpRequestMessage.Method = new HttpMethod(httpVerb.ToString());
            httpRequestMessage.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse(httpContentType));
            httpRequestMessage.RequestUri = serviceUri;

            return httpRequestMessage;
        }
    }
}
