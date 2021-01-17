namespace ZTR.Framework.Security
{
    using System;
    using System.Net.Http;

    public static class HttpRequestMessageExtension
    {
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
