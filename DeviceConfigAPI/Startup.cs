namespace Service
{
    using EnsureThat;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Converters;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration;
    using ZTR.Framework.Security;
    using ZTR.Framework.Service;

    /// <summary>
    ///   Added Startup class.
    /// </summary>
    public class Startup
    {
        private static ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">service collection.</param>
#pragma warning disable CA1822 // Mark members as static
        public void ConfigureServices(IServiceCollection services)
#pragma warning restore CA1822 // Mark members as static
        {
            services.AddControllers();

#if RELEASE
            services.AddAllowAllOriginsCorsPolicy();
#endif

            services.AddMvc()
                .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()))
                .AddXmlSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var swaggerAssemblies = new[]
            {
                typeof(Program).Assembly,
                typeof(Model).Assembly,
            };

            services.AddSwaggerWithComments(ApiConstants.ApiName, ApiConstants.ApiVersion, ApiConstants.ApiDescription, swaggerAssemblies);

            // we add our custom services here.
            services.AddServices();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">application builder.</param>
#pragma warning disable CA1822 // Mark members as static
        public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822 // Mark members as static
        {
            EnsureArg.IsNotNull(app);
            if (ApplicationConfiguration.IsDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseMiddleware<ForceHttpsMiddleware>();
                app.UseHttpsRedirection();
            }

            // Use routing first, then Cors second.
            app.UseRouting();
            var serviceProviderBuilt = app.ApplicationServices;

            logger = serviceProviderBuilt.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Program));

            // Use Exception middleware.
            app.AddProblemDetailsSupport();

            // Cors needs to be before MVC and Swagger. Otherwise typescript clients throw cors related exceptions.
            logger.LogWarning($"AllowAllOrigins true, so all origins are allowed");
            logger.LogWarning("Caution: Use this setting in DEVELOPMENT only. In production, grant access to specific origins (websites) that you control and trust to access the API.");

            var securityOptions = app.ApplicationServices.GetRequiredService<SecurityOptions>();

            // Cors needs to be before MVC and Swagger. Otherwise typescript clients throw cors related exceptions.
            if (securityOptions.AllowAllOrigins)
            {
                app.UseCors(ApiConstants.ApiAllowAllOriginsPolicy);
            }

            app.UseSwagger(new[]
            {
                new SwaggerConfigurationModel(ApiConstants.ApiVersion, ApiConstants.ApiName, true),
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
