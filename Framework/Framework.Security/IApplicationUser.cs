namespace ZTR.Framework.Security
{
    /// <summary>
    /// IApplicationUser
    /// </summary>
    public interface IApplicationUser
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        long UserId { get; set; }

        /// <summary>
        /// Gets or sets the format iso code.
        /// </summary>
        /// <value>
        /// The format iso code.
        /// </value>
        string FormatIsoCode { get; set; }

        /// <summary>
        /// Gets or sets the current time zone.
        /// </summary>
        /// <value>
        /// The current time zone.
        /// </value>
        string CurrentTimeZone { get; set; }
    }
}
