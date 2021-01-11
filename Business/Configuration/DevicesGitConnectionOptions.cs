namespace Business.Configuration
{
    using ZTR.Framework.Business;

    /// <summary>
    /// This class maps to configuration in appsettings.json file.
    /// </summary>
    /// <seealso cref="GitConnectionOptions" />
    public sealed class DevicesGitConnectionOptions : GitConnectionOptions
    {
        /// <summary>
        /// Gets or sets the device toml.
        /// </summary>
        /// <value>
        /// The device toml.
        /// </value>
        public string DeviceToml { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicesGitConnectionOptions"/> class.
        /// </summary>
        public DevicesGitConnectionOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicesGitConnectionOptions"/> class.
        /// </summary>
        /// <param name="gitLocalFolder">The git local folder.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="gitRepositoryUrl">The git repository URL.</param>
        public DevicesGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl) :
            base(gitLocalFolder, userName, password, gitRepositoryUrl)
        {
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"DevicesGitConnectionOptions($ GitLocalFolder: {GitLocalFolder} GitRepoUrl: {GitRemoteLocation})";
        }
    }
}
