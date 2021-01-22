namespace Service
{
    using Business.GitRepository.Interfaces;
    using Business.GitRepository.Managers;
    using Business.GitRepositoryWrappers.Interfaces;
    using Business.GitRepositoryWrappers.Managers;
    using Business.Parsers;
    using Business.RequestHandlers.Interfaces;
    using Business.RequestHandlers.Managers;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using ZTR.Framework.Service;

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

            services.AddScoped<IGitRepositoryManager, GitRepositoryManager>();
            services.AddScoped<IDeviceTypeManager, DeviceTypeManager>();
            services.AddScoped<IModuleManager, ModuleManager>();
            services.AddScoped<IFirmwareVersionManager, FirmwareVersionManager>();
            services.AddScoped<IDefaultValueManager, DefaultValueManager>();
            services.AddScoped<IBlockManager, BlockManager>();
            services.AddScoped<IConfigManager, ConfigManager>();
            services.AddScoped<IConfigCreateFromManager, ConfigCreateFromManager>();
            services.AddScoped<ICompatibleFirmwareVersionManager, CompatibleFirmwareVersionManager>();

            services.AddScoped<IModuleServiceManager, ModuleServiceManager>();
            services.AddScoped<IDeviceServiceManager, DeviceServiceManager>();
            services.AddScoped<IBlockServiceManager, BlockServiceManager>();
            services.AddScoped<IFirmwareVersionServiceManager, FirmwareVersionServiceManager>();

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
