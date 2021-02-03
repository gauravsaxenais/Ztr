namespace Business.GitRepository.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
        /// Clones the git hub repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitRepoAsync();

        /// <summary>
        /// Sets the git repo URL.
        /// </summary>
        /// <param name="gitUrl">The git URL.</param>
        void SetGitRepoUrl(string gitUrl);
    }
}
