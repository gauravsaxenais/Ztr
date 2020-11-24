namespace Business.Configuration
{
    using System.IO;
    using ZTR.Framework.Business.File;
    public sealed class BlockGitConnectionOptions : GitConnectionOptions
    {
        public BlockGitConnectionOptions()
        {
        }

        // username and password is empty as view and clone
        // options doesnot require login on ZTR network.
        public BlockGitConnectionOptions(string gitLocalFolder, string userName, string password, string gitRepositoryUrl) :
            base(gitLocalFolder, userName, password, gitRepositoryUrl)
        {
            SetConnection();
        }

        public void SetConnection()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            this.GitLocalFolder = Path.Combine(currentDirectory, this.GitLocalFolder);
        }

        public override string ToString()
        {
            return $"BlockGitConnectionOptions(${this.GitLocalFolder} {this.GitRepositoryUrl})";
        }
    }
}
