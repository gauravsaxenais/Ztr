namespace ZTR.Framework.Security
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ClientCredentialsSecurityOptions
    /// </summary>
    public class ClientCredentialsSecurityOptions
    {
        /// <summary>
        /// Gets or sets the credentials client identifier.
        /// </summary>
        /// <value>
        /// The credentials client identifier.
        /// </value>
        public string CredentialsClientId { get; set; }

        /// <summary>
        /// Gets or sets the credentials client secret.
        /// </summary>
        /// <value>
        /// The credentials client secret.
        /// </value>
        public Guid CredentialsClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> Scopes { get; set; }
    }
}
