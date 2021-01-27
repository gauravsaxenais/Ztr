namespace Business.GitRepository.Interfaces
{
    using Common.Configuration;
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
        /// Sets the git repo connection.
        /// </summary>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        void SetGitRepoConnection(ModuleBlockGitConnectionOptions moduleGitConnectionOptions);

        /// <summary>
        /// Clones the git repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitRepoAsync();
    }
}
