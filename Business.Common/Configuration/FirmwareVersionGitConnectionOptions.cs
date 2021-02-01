namespace Business.Common.Configuration
{
    using ZTR.Framework.Configuration;

    /// <summary>
    /// This class maps to configuration in appsettings.json file.
    /// </summary>
    /// <seealso cref="GitConnectionOptions" />
    public sealed class FirmwareVersionGitConnectionOptions : IGitConnectionOptions
    {
        public string GitLocalFolder { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string GitRemoteLocation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionGitConnectionOptions"/> class.
        /// </summary>
        public FirmwareVersionGitConnectionOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareVersionGitConnectionOptions"/> class.
        /// </summary>
        /// <param name="gitLocalFolder">The git local folder.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="gitRepositoryUrl">The git repository URL.</param>
        public FirmwareVersionGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl)

        {
            GitLocalFolder = gitLocalFolder;
            UserName = userName;
            Password = password;
            GitRemoteLocation = gitRepositoryUrl;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"FirmwareVersionGitConnectionOptions($ GitLocalFolder: {GitLocalFolder} GitRepoUrl: {GitRemoteLocation})";
        }
    }
}
