namespace Service.Configuration
{
    using System;
    using ZTR.Framework.Configuration;

    /// <summary>
    /// Application Options class. This class maps to settings in
    /// appsettings.json.
    /// </summary>
    /// <seealso cref="ZTR.Framework.Configuration.ConfigurationOptions" />
    public sealed class ApplicationOptions : ConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the read only connection string.
        /// </summary>
        /// <value>
        /// The read only connection string.
        /// </value>
        public string ReadOnlyConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the service base URI.
        /// </summary>
        /// <value>
        /// The service base URI.
        /// </value>
        public Uri ServiceBaseUri { get; set; }
    }
}
