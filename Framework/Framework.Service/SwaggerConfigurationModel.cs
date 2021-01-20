namespace ZTR.Framework.Service
{
    /// <summary>
    /// SwaggerConfigurationModel
    /// </summary>
    public class SwaggerConfigurationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerConfigurationModel"/> class.
        /// </summary>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="apiName">Name of the API.</param>
        /// <param name="alwaysShowInSwaggerUi">if set to <c>true</c> [always show in swagger UI].</param>
        public SwaggerConfigurationModel(string apiVersion, string apiName, bool alwaysShowInSwaggerUi = false)
        {
            ApiVersion = apiVersion;
            ApiName = apiName;
            AlwaysShowInSwaggerUI = alwaysShowInSwaggerUi;
        }

        /// <summary>
        /// Gets a value indicating whether [always show in swagger UI].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [always show in swagger UI]; otherwise, <c>false</c>.
        /// </value>
        public bool AlwaysShowInSwaggerUI { get; }

        /// <summary>
        /// Gets the API version.
        /// </summary>
        /// <value>
        /// The API version.
        /// </value>
        public string ApiVersion { get; }

        /// <summary>
        /// Gets the name of the API.
        /// </summary>
        /// <value>
        /// The name of the API.
        /// </value>
        public string ApiName { get; }
    }
}
