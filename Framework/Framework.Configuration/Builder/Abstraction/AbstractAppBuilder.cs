namespace ZTR.Framework.Configuration.Builder.Abstraction
{
    using System;
    using System.Collections.Generic;
    using Extension;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Abstract app builder class.
    /// </summary>
    public abstract class AbstractAppBuilder
    {
        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="hostingEnvironmentName">Name of the hosting environment.</param>
        /// <param name="configurationBuilder">The configuration builder.</param>
        /// <param name="types">The types.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="args">The arguments.</param>
        public virtual void ConfigureApp(string hostingEnvironmentName, IConfigurationBuilder configurationBuilder, IEnumerable<Type> types, List<IConfigurationOptions> configurationOptions, string[] args = null)
        {
            configurationBuilder.AddAppConfiguration(hostingEnvironmentName, args);
            var configurationRoot = configurationBuilder.Build();
            foreach (var optionType in types)
            {
                var instance = (ConfigurationOptions)Activator.CreateInstance(optionType);
                configurationRoot.GetSection(instance.SectionName).Bind(instance);
                configurationOptions.Add(instance);
            }
        }
    }
}
