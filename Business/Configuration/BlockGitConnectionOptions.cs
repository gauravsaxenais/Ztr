namespace Business.Configuration
{
    using ZTR.Framework.Business.File;
    public sealed class BlockGitConnectionOptions : GitConnectionOptions
    {
        public BlockGitConnectionOptions()
        {
        }

        // username and password is empty as view and clone
        // options doesnot require login on ZTR network.
        public BlockGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl, string blockLocalFolder) :
            base(gitLocalFolder, userName, password, gitRepositoryUrl)
        {
            BlockLocalFolder = blockLocalFolder;
        }

        public string BlockLocalFolder { get; set; }
        
        public override string ToString()
        {
            return $"BlockGitConnectionOptions(${this.GitLocalFolder} {this.GitRepositoryUrl} {this.BlockLocalFolder})";
        }
    }
}
