namespace Business.GitRepository.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Git repo wrapper for devices.
    /// </summary>
    public interface IFirmwareVersionServiceManager
    {
        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllFirmwareVersionsAsync();

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <param name="connectionOptions">The connection options.</param>
        void SetGitRepoConnection(GitConnectionOptions connectionOptions);
    }
}
