namespace Business.GitRepository.Interfaces
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Block Service Manager.
    /// </summary>
    public interface IBlockServiceManager
    {
        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        Task<List<FileInfo>> GetAllBlockFilesAsync();

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitRepoAsync();
    }
}
