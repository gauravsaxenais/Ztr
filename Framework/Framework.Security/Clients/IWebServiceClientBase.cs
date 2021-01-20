namespace ZTR.Framework.Security
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// IWebServiceClientBase
    /// </summary>
    public interface IWebServiceClientBase
    {
        // Have to use Func<HttpRequestMessage, Task> instead of Action<HttpRequestMessage> since Action<..> won't wait the method to finish        
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
    }
}
