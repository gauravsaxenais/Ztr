namespace ZTR.Framework.Security
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// HttpClientExtension
    /// </summary>
    public static class HttpClientExtension
    {
        /// <summary>
        /// Configurations the HTTP client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="apiTimeoutInMinutes">The API timeout in minutes.</param>
        /// <param name="userAgentHeaders">The user agent headers.</param>
        /// <returns></returns>
        public static HttpClient ConfigHttpClient(this HttpClient client, long? apiTimeoutInMinutes, string userAgentHeaders)
        {
            client.Timeout = apiTimeoutInMinutes.HasValue ? TimeSpan.FromMinutes(apiTimeoutInMinutes.Value) : client.Timeout;
            client.DefaultRequestHeaders.Add("User-Agent", userAgentHeaders);

            return client;
        }
    }
}
