namespace Service
{
    using Business.RequestHandlers.Interfaces;
    using Business.RequestHandlers.Managers;
    using EnsureThat;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Services Collections Extensions.
    /// Inject services in the application here.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>Adds the services.</summary>
        /// <param name="services">The services.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.AddScoped<IGitRepositoryManager, GitRepositoryManager>();

            services.AddScoped<IModuleManager, ModuleManager>();
            services.AddScoped<IDeviceTypeManager, DeviceTypeManager>();
            services.AddScoped<IDefaultValueManager, DefaultValueManager>();
            services.AddScoped<IBlockManager, BlockManager>();
            services.AddScoped<IConfigGeneratorManager, ConfigGeneratorManager>();

            return services;
        }

        /// <summary>
        /// Add CORS policy for the project.
        /// </summary>
        /// <param name="services">services collection.</param>
        public static void AddAllowAllOriginsCorsPolicy(this IServiceCollection services)
        {
            // Setup CORS
            var corsBuilder = new CorsPolicyBuilder();

            corsBuilder.AllowAnyOrigin(); // For anyone access.
            corsBuilder.AllowAnyMethod();
            corsBuilder.AllowAnyHeader();
            corsBuilder.AllowCredentials();

            services.AddCors(options =>
            {
                options.AddPolicy(ApiConstants.ApiAllowAllOriginsPolicy, corsBuilder.Build());
            });
        }
    }
}
