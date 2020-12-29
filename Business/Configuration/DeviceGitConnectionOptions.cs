namespace Business.Configuration
{
    using ZTR.Framework.Business.File;

    /// <summary>
    /// This class maps to the connection options in appsettings.json file.
    /// </summary>
    public sealed class DeviceGitConnectionOptions : GitConnectionOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceGitConnectionOptions"/> class.
        /// </summary>
        public DeviceGitConnectionOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceGitConnectionOptions"/> class.
        /// </summary>
        /// <param name="gitLocalFolder">The git local folder.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="gitRepositoryUrl">The git repository URL.</param>
        /// <param name="tomlConfiguration">The toml configuration.</param>
        public DeviceGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl, TomlConfigurationFile tomlConfiguration) :
            base(gitLocalFolder, userName, password, gitRepositoryUrl)
        {
            DefaultTomlConfiguration = tomlConfiguration;
        }

        /// <summary>
        /// Gets or sets the toml configuration.
        /// </summary>
        /// <value>
        /// The toml configuration.
        /// </value>
        public TomlConfigurationFile DefaultTomlConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the modules configuration folder.
        /// </summary>
        /// <value>
        /// The modules configuration.
        /// </value>
        public string ModulesConfig { get; set; }

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
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"DeviceGitConnectionOptions($ GitLocalFolder: {this.GitLocalFolder} GitRepoUrl: {this.GitRepositoryUrl} TomlConfiguration: {this.DefaultTomlConfiguration})";
        }
    }
}