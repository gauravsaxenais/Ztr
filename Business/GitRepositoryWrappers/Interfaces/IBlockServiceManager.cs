namespace Business.GitRepositoryWrappers.Interfaces
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
        Task<IEnumerable<FileInfo>> GetAllBlockFilesAsync();

        /// <summary>
        /// Gets all modules.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllModulesAsync();

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitRepoAsync();
    }
}
