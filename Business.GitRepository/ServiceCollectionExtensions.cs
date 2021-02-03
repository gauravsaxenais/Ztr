namespace Business.GitRepository
{
    using Business.GitRepository.Interfaces;
    using Business.GitRepository.Managers;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static void AddGitConnections(this IServiceCollection services)
        {
            // Service managers for git repository.
            services.AddTransient<IModuleServiceManager, ModuleServiceManager>();
            services.AddTransient<IDeviceServiceManager, DeviceServiceManager>();
            services.AddTransient<IBlockServiceManager, BlockServiceManager>();
            services.AddTransient<IFirmwareVersionServiceManager, FirmwareVersionServiceManager>();
        }
    }
}
