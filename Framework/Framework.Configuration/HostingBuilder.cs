namespace ZTR.Framework.Configuration
{
    using System;
    using System.Collections.Generic;
    using Builder.Abstraction;
    using Builder.Extension;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostingBuilder class.
    /// </summary>
    /// <seealso cref="AbstractAppBuilder" />
    public class HostingBuilder : AbstractAppBuilder, IAppBuilder<IHostBuilder>
    {
        private const string EnvironmentVariablePrefix = "ASPNETCORE_";

        /// <summary>
        /// Configures the host configuration.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <returns></returns>
        public IHostBuilder ConfigureHostConfiguration(IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureHostConfiguration(configuration =>
            {
                configuration.AddEnvironmentVariables(prefix: EnvironmentVariablePrefix);
            });
        }

        /// <summary>
        /// Configures the application configuration.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="types">The types.</param>
        /// <param name="options">The options.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public IHostBuilder ConfigureAppConfiguration(IHostBuilder hostBuilder, IEnumerable<Type> types, List<IConfigurationOptions> options, string[] args)
        {
            return hostBuilder.ConfigureAppConfiguration((webHostBuilderContext, builderConfiguration) =>
            {
                ConfigureApp(webHostBuilderContext.HostingEnvironment.EnvironmentName, builderConfiguration, types, options, args);
            });
        }

        /// <summary>
        /// Configures the logging.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="loggingSectionName">Name of the logging section.</param>
        /// <returns></returns>
        public IHostBuilder ConfigureLogging(IHostBuilder hostBuilder, string loggingSectionName)
        {
            return hostBuilder.ConfigureLogging((webHostBuilderContext, logging) =>
            {
                var configuration = webHostBuilderContext.Configuration.GetSection(loggingSectionName);
                logging.AddLogging(configuration);
            });
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <returns></returns>
        public IHostBuilder ConfigureServices(IHostBuilder hostBuilder, List<IConfigurationOptions> configurationOptions)
        {
            return hostBuilder.ConfigureServices((hostingContext, services) =>
            {
                configurationOptions.ForEach(x => services.AddSingleton(x.GetType(), x));
            });
        }
    }
}
