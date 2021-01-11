namespace ZTR.Framework.Business
{
    using Configuration;

    public class GitConnectionOptions : ConfigurationOptions
    {
        // for serialization
        public GitConnectionOptions()
        {
        }

        public GitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRemoteLocation)
        {
            GitLocalFolder = gitLocalFolder;
            UserName = userName;
            Password = password;
            GitRemoteLocation = gitRemoteLocation;
        }

        public string GitLocalFolder { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string GitRemoteLocation { get; set; }
        public override string ToString()
        {
            return $"GitConnectionOptions(${GitLocalFolder} {UserName} {GitRemoteLocation})";
        }
    }
}
