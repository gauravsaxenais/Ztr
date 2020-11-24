namespace Business.Configuration
{
    using ZTR.Framework.Business.File;
    public sealed class DeviceGitConnectionOptions : GitConnectionOptions
    {
        public DeviceGitConnectionOptions()
        {
        }

        // username and password is empty as view and clone
        // options doesnot require login on ZTR network.
        public DeviceGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl, TomlConfigurationFile tomlConfiguration) :
            base(gitLocalFolder, userName, password, gitRepositoryUrl)
        {
            TomlConfiguration = tomlConfiguration;
        }

        public TomlConfigurationFile TomlConfiguration { get; set; }
        
        public override string ToString()
        {
            return $"DeviceGitConnectionOptions(${this.GitLocalFolder} {this.GitRepositoryUrl} {this.TomlConfiguration})";
        }
    }
}
