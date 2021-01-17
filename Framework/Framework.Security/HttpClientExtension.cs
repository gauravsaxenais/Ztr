namespace ZTR.Framework.Security
{
    using System;
    using System.Net.Http;

    public static class HttpClientExtension
    {
        public static HttpClient ConfigHttpClient(this HttpClient client, long? apiTimeoutInMinutes, string userAgentHeaders)
        {
            client.Timeout = apiTimeoutInMinutes.HasValue ? TimeSpan.FromMinutes(apiTimeoutInMinutes.Value) : client.Timeout;
            client.DefaultRequestHeaders.Add("User-Agent", userAgentHeaders);

            return client;
        }
    }
}
