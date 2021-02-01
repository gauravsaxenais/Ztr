namespace ZTR.Framework.Configuration
{
    /// <summary>
    /// IGitConnectionOptions.
    /// </summary>
    public interface IGitConnectionOptions
    {
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
    }
}
