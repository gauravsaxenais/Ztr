namespace ZTR.Framework.Security
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// TokenSecurityOptions
    /// </summary>
    public class TokenSecurityOptions
    {
        /// <summary>
        /// Gets or sets the API code.
        /// </summary>
        /// <value>
        /// The API code.
        /// </value>
        public string ApiCode { get; set; }

        /// <summary>
        /// Gets or sets the API secret.
        /// </summary>
        /// <value>
        /// The API secret.
        /// </value>
        public Guid ApiSecret { get; set; }

        /// <summary>
        /// Gets or sets the swagger client identifier.
        /// </summary>
        /// <value>
        /// The swagger client identifier.
        /// </value>
        public string SwaggerClientId { get; set; }

        /// <summary>
        /// Gets or sets the swagger client secret.
        /// </summary>
        /// <value>
        /// The swagger client secret.
        /// </value>
        public Guid SwaggerClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> Scopes { get; set; }
    }
}
