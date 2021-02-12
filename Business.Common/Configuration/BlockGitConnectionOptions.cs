namespace Business.Common.Configuration
{
    using ZTR.Framework.Configuration;

    /// <summary>
    /// This class maps to the connection options in appsettings.json file.
    /// </summary>
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
        /// <param name="tomlConfiguration">The toml configuration.</param>
        public BlockGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl) :
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
            return $"BlockGitConnectionOptions($ GitLocalFolder: {GitLocalFolder} GitRepoUrl: {GitRemoteLocation} )";
        }
    }
}