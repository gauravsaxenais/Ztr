namespace ZTR.Framework.Service
{
    using EnsureThat;
    using global::Service;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ZTR.Framework.Service.ExceptionLogger;

    /// <summary>ApplicationBuilder.</summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the application custom build.
        /// </summary>
        /// <param name="app">The application.</param>
        public static void AddAppCustomBuild(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            app.UseMiddleware<ExceptionMiddleware>();

            // Cors needs to be before MVC and Swagger. Otherwise typescript clients throw cors related exceptions.
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Program));
            logger.LogWarning($"AllowAllOrigins true, so all origins are allowed");
            logger.LogWarning("Caution: Use this setting in DEVELOPMENT only. In production, grant access to specific origins (websites) that you control and trust to access the API.");

            app.UseCors(ApiConstants.ApiAllowAllOriginsPolicy);
        }
    }
}
