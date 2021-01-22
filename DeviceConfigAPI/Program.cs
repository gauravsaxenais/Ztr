namespace Service
{
    using Business.Configuration;
    using Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using System;
    using ZTR.Framework.Configuration;
    using ZTR.Framework.Security;
    using ZTR.Framework.Service;

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

            Log.Logger = new LoggerConfiguration()
                           .WriteTo.Console()
                           .WriteTo.File("Logs/Log-.txt", rollingInterval: RollingInterval.Hour)
                           .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .DefaultAppConfiguration(
                new[]
                {
                    typeof(ApplicationOptions).Assembly,
                    typeof(SecurityOptions).Assembly,
                    typeof(ModuleBlockGitConnectionOptions).Assembly,
                    typeof(DeviceGitConnectionOptions).Assembly
                }, args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}