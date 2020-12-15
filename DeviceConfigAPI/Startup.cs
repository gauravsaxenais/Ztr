namespace Service
{
    using EnsureThat;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Converters;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration;
    using ZTR.Framework.Service;

    /// <summary>
    ///   Added Startup class.
    /// </summary>
    public class Startup
    {
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

            // Add cors here.
            services.AddCors(o => o.AddPolicy(ApiConstants.ApiAllowAllOriginsPolicy, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // we add our custom services here.
            services.AddCustomServices();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">application builder.</param>
        public void Configure(IApplicationBuilder app)
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
            app.AddAppCustomBuild();

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
