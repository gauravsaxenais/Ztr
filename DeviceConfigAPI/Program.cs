namespace Service
{
    using System;
    using Business.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Service.Configuration;
    using ZTR.Framework.Configuration;
    using ZTR.Framework.Security;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class Program
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        public static void Main(string[] args)
        {
            Console.Title = ApiConstants.ApiName;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .DefaultAppConfiguration(
                new[]
                {
                    typeof(ApplicationOptions).Assembly,
                    typeof(SecurityOptions).Assembly,
                    typeof(DeviceGitConnectionOptions).Assembly,
                    typeof(ModuleGitConnectionOptions).Assembly,
                    typeof(BlockGitConnectionOptions).Assembly,
                }, args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}