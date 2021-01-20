namespace ZTR.Framework.Service
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// ForceHttpsMiddleware
    /// </summary>
    public class ForceHttpsMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForceHttpsMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        public ForceHttpsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            request.Scheme = "https";

            await _next(context).ConfigureAwait(false);
        }
    }
}
