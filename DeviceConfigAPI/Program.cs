namespace Service
{
    using Business.Common.Configuration;
    using Business.Configuration;
    using Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using System;
    using ZTR.Framework.Configuration;
    using ZTR.Framework.Configuration.Builder.Extension;
    using ZTR.Framework.Security;
    using ZTR.Framework.Service;

    /// <summary>
    ///   Entry point of the application.
    /// </summary>
    public class Program
    {
        private const string JsonFileName = "appsettings.json";
        private static string _environment = EnvironmentVariable.Development.ToString();

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.Title = ApiConstants.ApiName;
            SetEnvironment();

            // logging has been added from LoggingExtensions using Serilog.
            // refer to Framework.Configuration -> Extension -> LoggingExtension.
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
                .ConfigureAppConfiguration((hostingContext, configuration) => configuration.AddAppConfiguration(_environment, null))
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

        private static void SetEnvironment()
        {
            try
            {
                var config = new ConfigurationBuilder().AddJsonFile(JsonFileName, false).Build();
                _environment = config.GetSection("Environment").Value;
            }
            catch (Exception)
            {
                _environment = EnvironmentVariable.Development.ToString();
            }
        }
    }
}