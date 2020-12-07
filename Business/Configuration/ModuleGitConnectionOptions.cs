namespace Business.Configuration
{
    using ZTR.Framework.Business.File;

    /// <summary>
    /// This class maps to configuration in appsettings.json file.
    /// </summary>
    /// <seealso cref="ZTR.Framework.Business.File.GitConnectionOptions" />
    public sealed class ModuleGitConnectionOptions : GitConnectionOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleGitConnectionOptions"/> class.
        /// </summary>
        public ModuleGitConnectionOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleGitConnectionOptions"/> class.
        /// </summary>
        /// <param name="gitLocalFolder">The git local folder.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="gitRepositoryUrl">The git repository URL.</param>
        public ModuleGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl) :
            base(gitLocalFolder, userName, password, gitRepositoryUrl)
        {
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"ModuleGitConnectionOptions(${this.GitLocalFolder} {this.GitRepositoryUrl})";
        }
    }
}
