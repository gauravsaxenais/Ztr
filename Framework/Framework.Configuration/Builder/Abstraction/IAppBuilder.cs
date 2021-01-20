namespace ZTR.Framework.Configuration.Builder.Abstraction
{
    using System;
    using System.Collections.Generic;

    #pragma warning disable CS1591
    public interface IAppBuilder<TBuilder>
    {
        /// <summary>
        /// Configures the host configuration.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        TBuilder ConfigureHostConfiguration(TBuilder builder);

        /// <summary>
        /// Configures the application configuration.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="types">The types.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        TBuilder ConfigureAppConfiguration(TBuilder builder, IEnumerable<Type> types, List<IConfigurationOptions> configurationOptions, string[] args);

        /// <summary>
        /// Configures the logging.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="loggingSectionName">Name of the logging section.</param>
        /// <returns></returns>
        TBuilder ConfigureLogging(TBuilder builder, string loggingSectionName);

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <returns></returns>
        TBuilder ConfigureServices(TBuilder builder, List<IConfigurationOptions> configurationOptions);
    }
    #pragma warning restore CS1591
}
