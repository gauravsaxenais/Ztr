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

    /// <summary>
    ///   Entry point of the application.
    /// </summary>
    public class Program
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.Title = ApiConstants.ApiName;
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>hostbuilder.</returns>
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