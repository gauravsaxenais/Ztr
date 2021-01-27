namespace Business.GitRepository.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Git repo wrapper for devices.
    /// </summary>
    public interface IDeviceServiceManager
    {
        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitHubRepoAsync();

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllDevicesAsync(string filePath);

        /// <summary>
        /// Gets the list of devices asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        Task<List<Dictionary<string, object>>> GetListOfDevicesAsync(string filePath);

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <param name="devicesGitConnectionOptions">The devices git connection options.</param>
        void SetGitRepoConnection(GitConnectionOptions devicesGitConnectionOptions);
    }
}
