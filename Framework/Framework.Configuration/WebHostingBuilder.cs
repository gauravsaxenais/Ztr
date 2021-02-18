namespace ZTR.Framework.Configuration
{
    using System;
    using System.Collections.Generic;
    using Builder.Abstraction;
    using Builder.Extension;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// WebHostingBuilder
    /// </summary>
    /// <seealso cref="AbstractAppBuilder" />
    public class WebHostingBuilder : AbstractAppBuilder, IAppBuilder<IWebHostBuilder>
    {
        /// <summary>
        /// Configures the host configuration.
        /// </summary>
        /// <param name="webHostBuilder">The web host builder.</param>
        /// <returns></returns>
        public IWebHostBuilder ConfigureHostConfiguration(IWebHostBuilder webHostBuilder)
        {
            return webHostBuilder;
        }

        /// <summary>
        /// Configures the application configuration.
        /// </summary>
        /// <param name="webHostBuilder">The web host builder.</param>
        /// <param name="types">The types.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public IWebHostBuilder ConfigureAppConfiguration(IWebHostBuilder webHostBuilder, IEnumerable<Type> types, List<IConfigurationOptions> configurationOptions, string[] args)
        {
            return webHostBuilder.ConfigureAppConfiguration((webHostBuilderContext, builderConfiguration) =>
            {
                ConfigureApp(webHostBuilderContext.HostingEnvironment.EnvironmentName, builderConfiguration, types, configurationOptions, args);
            });
        }

        /// <summary>
        /// Configures the logging.
        /// </summary>
        /// <param name="webHostBuilder">The web host builder.</param>
        /// <param name="loggingSectionName">Name of the logging section.</param>
        /// <returns></returns>
        public IWebHostBuilder ConfigureLogging(IWebHostBuilder webHostBuilder, string loggingSectionName = "Logging")
        {
            return webHostBuilder.ConfigureServices((webHostBuilderContext, collection) =>
            {
                var configuration = webHostBuilderContext.Configuration.GetSection(loggingSectionName);

                collection.AddLogging(builder =>
                {
                    builder.AddLogging(configuration);
                });
            });
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="webHostBuilder">The web host builder.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <returns></returns>
        public IWebHostBuilder ConfigureServices(IWebHostBuilder webHostBuilder, List<IConfigurationOptions> configurationOptions)
        {
            return webHostBuilder.ConfigureServices((hostingContext, services) =>
            {
                configurationOptions.ForEach(x => services.AddSingleton(x.GetType(), x));
            });
        }
    }
}
