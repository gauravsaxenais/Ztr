namespace ZTR.Framework.Service
{
    /// <summary>Constants for API.</summary>
    public static class ApiConstants
    {
        /// <summary>
        /// The API name.
        /// </summary>
        public const string ApiName = "ZTR M7 API";

        /// <summary>
        /// The API version.
        /// </summary>
        public const string ApiVersion = "v1";

        /// <summary>
        /// The API description.
        /// </summary>
        public const string ApiDescription = "Get all the available devices, gets all the available firmware versions for a device, displays all the compatible firmware versions for a particular firmware version, displays all the modules and their default values, displays all the blocks, takes an input for J1939 html file, takes existing config.toml as an input and finally generates a config.toml. Data is available in JSON format.";

        /// <summary>
        /// The API allow all origins policy.
        /// </summary>
        public const string ApiAllowAllOriginsPolicy = "ZTRAllowAllOrigins";
    }
}
