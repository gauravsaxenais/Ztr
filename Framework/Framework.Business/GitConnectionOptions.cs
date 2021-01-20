namespace ZTR.Framework.Business
{
    using Configuration;

    /// <summary>
    /// A base class for GitConnectionOptions.
    /// </summary>
    /// <seealso cref="ZTR.Framework.Configuration.ConfigurationOptions" />
    public class GitConnectionOptions : ConfigurationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitConnectionOptions"/> class.
        /// </summary>
        public GitConnectionOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitConnectionOptions"/> class.
        /// </summary>
        /// <param name="gitLocalFolder">The git local folder.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="gitRemoteLocation">The git remote location.</param>
        public GitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRemoteLocation)
        {
            GitLocalFolder = gitLocalFolder;
            UserName = userName;
            Password = password;
            GitRemoteLocation = gitRemoteLocation;
        }

        /// <summary>
        /// Gets or sets the git local folder.
        /// </summary>
        /// <value>
        /// The git local folder.
        /// </value>
        public string GitLocalFolder { get; set; }
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets the git remote location.
        /// </summary>
        /// <value>
        /// The git remote location.
        /// </value>
        public string GitRemoteLocation { get; set; }
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"GitConnectionOptions(${GitLocalFolder} {UserName} {GitRemoteLocation})";
        }
    }
}
