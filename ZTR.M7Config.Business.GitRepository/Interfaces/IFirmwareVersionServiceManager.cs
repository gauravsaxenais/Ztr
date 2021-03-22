namespace ZTR.M7Config.Business.GitRepository.Interfaces
{
    using ZTR.M7Config.Business.Common.Models;
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
        Task<List<string>> GetAllFirmwareVersionsAsync();

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitRepoAsync();

        /// <summary>
        /// Sets the git repo URL.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="gitUrl">The git URL.</param>
        void SetGitRepoUrl(string deviceType, string gitUrl);

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
        /// Gets the tags with no device file modified.
        /// </summary>
        /// <param name="tagList">The tag list.</param>
        /// <param name="mainTag">The main tag.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="modules">The modules.</param>
        /// <returns></returns>
        Task<List<string>> GetCompatibleFirmwareVersions(List<string> tagList, string mainTag, string deviceType, List<ModuleReadModel> modules);

        /// <summary>
        /// Gets the firmware URL asynchronous.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<string> GetFirmwareUrlAsync(string deviceType);
    }
}
