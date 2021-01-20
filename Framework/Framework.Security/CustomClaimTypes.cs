namespace ZTR.Framework.Security
{
    /// <summary>
    /// CustomClaimTypes
    /// </summary>
    public static class CustomClaimTypes
    {
        /// <summary>
        /// The company master key
        /// </summary>
        public const string CompanyMasterKey = "company_master_key";

        /// <summary>
        /// The company identifier
        /// </summary>
        public const string CompanyId = "company_id_key";

        /// <summary>
        /// The language iso code
        /// </summary>
        public const string LanguageIsoCode = "language_iso_code";

        /// <summary>
        /// The format iso code
        /// </summary>
        public const string FormatIsoCode = "format_iso_code";

        /// <summary>
        /// The login provider type code
        /// </summary>
        public const string LoginProviderTypeCode = "login_type_code";

        /// <summary>
        /// The login provider code
        /// </summary>
        public const string LoginProviderCode = "login_provider_code";

        /// <summary>
        /// The is password recovery
        /// </summary>
        public const string IsPasswordRecovery = "password_recovery";

        /// <summary>
        /// The security rights
        /// </summary>
        public const string SecurityRights = "security_rights";

        /// <summary>
        /// The time zone
        /// </summary>
        public const string TimeZone = "time_zone";

        /// <summary>
        /// This is used during Client Credients flow to impersonate the user.
        /// It is not to be used during interactive flow.
        /// </summary>
        // TODO: change user key when upgrade to 3.1 --> "user_id_key";
        public const string UserId = nameof(UserId);
    }
}
