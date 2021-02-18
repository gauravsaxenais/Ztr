namespace ZTR.Framework.Configuration
{
    using System;
    using System.Collections.Generic;
    using Builder.Abstraction;

    /// <summary>
    /// Builder Creator class.
    /// </summary>
    public static class BuilderCreator
    {
        /// <summary>
        /// Constructs the specified application builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="appBuilder">The application builder.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="optionTypes">The option types.</param>
        /// <param name="options">The options.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="loggingSectionName">Name of the logging section.</param>
        /// <returns></returns>
        public static TBuilder Construct<TBuilder>(IAppBuilder<TBuilder> appBuilder, TBuilder builder, IEnumerable<Type> optionTypes, List<IConfigurationOptions> options, string[] args = null, string loggingSectionName = "Logging")
        {
            builder = appBuilder.ConfigureHostConfiguration(builder);
            builder = appBuilder.ConfigureAppConfiguration(builder, optionTypes, options, args);
            builder = appBuilder.ConfigureLogging(builder, loggingSectionName);
            builder = appBuilder.ConfigureServices(builder, options);
            return builder;
        }
    }
}
