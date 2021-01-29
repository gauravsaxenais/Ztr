namespace Business.GitRepository.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllDevicesAsync();

        /// <summary>
        /// Gets the list of devices asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<List<Dictionary<string, object>>> GetListOfDevicesAsync();
    }
}
