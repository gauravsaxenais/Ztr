namespace ZTR.Framework.Security
{
    using System;
    using System.Collections.Generic;
    using Configuration;

    /// <summary>
    /// SecurityOptions
    /// </summary>
    /// <seealso cref="ConfigurationOptions" />
    public class SecurityOptions : ConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the application module code.
        /// </summary>
        /// <value>
        /// The application module code.
        /// </value>
        public string ApplicationModuleCode { get; set; }

        /// <summary>
        /// Gets or sets the authority endpoint.
        /// </summary>
        /// <value>
        /// The authority endpoint.
        /// </value>
        public Uri AuthorityEndpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [require HTTPS metadata].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require HTTPS metadata]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireHttpsMetadata { get; set; }

        /// <summary>
        /// Gets or sets the cache duration seconds.
        /// </summary>
        /// <value>
        /// The cache duration seconds.
        /// </value>
        public double CacheDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the cookie security options.
        /// </summary>
        /// <value>
        /// The cookie security options.
        /// </value>
        public CookieSecurityOptions CookieSecurityOptions { get; set; }

        /// <summary>
        /// Converts to kensecurityoptions.
        /// </summary>
        /// <value>
        /// The token security options.
        /// </value>
        public TokenSecurityOptions TokenSecurityOptions { get; set; }

        /// <summary>
        /// Gets or sets the client credentials security options.
        /// </summary>
        /// <value>
        /// The client credentials security options.
        /// </value>
        public ClientCredentialsSecurityOptions ClientCredentialsSecurityOptions { get; set; }

        /// <summary>
        /// Gets the allowed origins.
        /// </summary>
        /// <value>
        /// The allowed origins.
        /// </value>
        public ICollection<Uri> AllowedOrigins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow all origins].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow all origins]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAllOrigins { get; set; }
    }
}
