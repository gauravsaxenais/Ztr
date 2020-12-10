namespace Business.Configuration
{
    using ZTR.Framework.Business.File;

    /// <summary>
    /// This class maps to the connection options in appsettings.json file.
    /// </summary>
    /// <seealso cref="GitConnectionOptions" />
    public sealed class BlockGitConnectionOptions : GitConnectionOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGitConnectionOptions"/> class.
        /// </summary>
        public BlockGitConnectionOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGitConnectionOptions"/> class.
        /// </summary>
        /// <param name="gitLocalFolder">The git local folder.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="gitRepositoryUrl">The git repository URL.</param>
        public BlockGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl) :
            base(gitLocalFolder, userName, password, gitRepositoryUrl)
        {
        }

        /// <summary>
        /// Gets or sets the block configuration.
        /// </summary>
        /// <value>
        /// The block configuration.
        /// </value>
        public string BlockConfig { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"BlockGitConnectionOptions($ GitLocalFolder: {this.GitLocalFolder} UserName:{this.UserName} GitRepoUrl:{this.GitRepositoryUrl})";
        }
    }
}
