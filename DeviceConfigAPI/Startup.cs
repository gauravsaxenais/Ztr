namespace Service
{
    using EnsureThat;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Newtonsoft.Json.Converters;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration;
    using ZTR.Framework.Service;
    using ZTR.Framework.Service.ExceptionLogger;
    using JsonSerializer = System.Text.Json.JsonSerializer;

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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));

            var swaggerAssemblies = new[]
            {
                typeof(Program).Assembly,
                typeof(Model).Assembly,
            };

            services.AddHealthChecks()
                .AddCheck<SystemMemoryHealthCheck>("health_check");

            services.AddSwaggerWithComments(ApiConstants.ApiName, ApiConstants.ApiVersion, ApiConstants.ApiDescription, swaggerAssemblies);

            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<FileUploadOperationFilter>();
            });

            // we add our custom services here.
            services.AddCustomServices();
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
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
            app.UseMiddleware<ExceptionMiddleware>();
            app.AddAppCorsAndExceptionMiddleWare();

            app.UseSwagger(new[]
            {
                new SwaggerConfigurationModel(ApiConstants.ApiVersion, ApiConstants.ApiName, true),
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    // This custom writer formats the detailed status as JSON.
                    ResponseWriter = WriteResponse
                });

                endpoints.MapControllers();
            });
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WriteString("status", result.Status.ToString());
                    writer.WriteStartObject("results");

                    foreach (var (key, value) in result.Entries)
                    {
                        writer.WriteStartObject(key);
                        writer.WriteString("status", value.Status.ToString());
                        writer.WriteString("description", value.Description);
                        writer.WriteStartObject("data");

                        foreach (var item in value.Data)
                        {
                            writer.WritePropertyName(item.Key);
                            JsonSerializer.Serialize(
                                writer, item.Value, item.Value?.GetType() ??
                                                    typeof(object));
                        }

                        writer.WriteEndObject();
                        writer.WriteEndObject();
                    }

                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());

                return context.Response.WriteAsync(json);
            }
        }
    }
}