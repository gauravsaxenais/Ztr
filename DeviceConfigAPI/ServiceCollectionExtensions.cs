namespace Service
{
    using Business.Parsers.ProtoParser;
    using Business.Parsers.TomlParser.Core.Converter;
    using Business.RequestHandlers.Interfaces;
    using Business.RequestHandlers.Managers;
    using EnsureThat;
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
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.AddSingleton<InputFileLoader>();

            services.AddScoped<IGitRepositoryManager, GitRepositoryManager>();
            services.AddScoped<IDeviceTypeManager, DeviceTypeManager>();
            services.AddScoped<IModuleManager, ModuleManager>();
            services.AddScoped<IDefaultValueManager, DefaultValueManager>();
            services.AddScoped<IBlockManager, BlockManager>();
            services.AddScoped<IConfigGeneratorManager, ConfigGeneratorManager>();
            services.AddScoped<IConfigCreateFromManager, ConfigCreateFromManager>();

            services.AddCors(o => o.AddPolicy(ApiConstants.ApiAllowAllOriginsPolicy, builder =>
            {
                builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            }));

            services.AddConverters();

            return services;
        }
    }
}
