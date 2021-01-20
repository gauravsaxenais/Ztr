namespace ZTR.Framework.Security
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// CookieSecurityOptions
    /// </summary>
    public class CookieSecurityOptions
    {
        /// <summary>
        /// Gets or sets the hybrid client identifier.
        /// </summary>
        /// <value>
        /// The hybrid client identifier.
        /// </value>
        public string HybridClientId { get; set; }

        /// <summary>
        /// Gets or sets the hybrid client secret.
        /// </summary>
        /// <value>
        /// The hybrid client secret.
        /// </value>
        public Guid HybridClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the name of the cookie.
        /// </summary>
        /// <value>
        /// The name of the cookie.
        /// </value>
        public string CookieName { get; set; }

        /// <summary>
        /// Gets or sets the signed out redirect URI.
        /// </summary>
        /// <value>
        /// The signed out redirect URI.
        /// </value>
        public Uri SignedOutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> Scopes { get; set; }
    }
}
