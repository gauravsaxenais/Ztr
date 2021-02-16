namespace Business.GitRepository.Interfaces
{
    using Business.Common.Models;
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

        /// <summary>
        /// Gets the default toml file content asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns></returns>
        Task<string> GetDefaultTomlFileContentAsync(string firmwareVersion);

        /// <summary>
        /// Gets the default toml file content asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<string> GetDeviceTomlFileContentAsync(string firmwareVersion);

        /// <summary>
        /// Gets the list of modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<List<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType);

        /// <summary>
        /// Gets the tags with device file modified.
        /// </summary>
        /// <param name="fromTags">From tags.</param>
        /// <param name="mainTag">The main tag.</param>
        /// <returns></returns>
        Task<List<string>> GetTagsWithNoDeviceFileModified(IEnumerable<string> fromTags, string mainTag);
    }
}
