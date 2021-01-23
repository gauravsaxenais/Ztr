namespace Business.GitRepository.Interfaces
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Block Service Manager.
    /// </summary>
    public interface IBlockServiceManager
    {
        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FileInfo>> GetAllBlockFilesAsync(string blockConfigPath);

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <param name="connectionOptions">The connection options.</param>
        void SetGitRepoConnection(GitConnectionOptions connectionOptions);

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitRepoAsync();
    }
}
