namespace ZTR.Framework.Configuration.Builder.Extension
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;

    #pragma warning disable CS1591
    public static class ConfigurationBuilderExtension
    {
        private const string JsonFileName = "appsettings.json";

        /// <summary>
        /// Adds the application configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="environmentName">Name of the environment.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddAppConfiguration(this IConfigurationBuilder configuration, string environmentName, string[] args)
        {
            var fileName = $"appsettings.{environmentName}.json";

            configuration.AddJsonFile(JsonFileName, false, true)
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), true, true)
                .AddJsonFile(fileName, true, true);

            if (args != null)
            {
                configuration.AddCommandLine(args);
            }

            return configuration;
        }
    }
    #pragma warning restore CS1591
}
