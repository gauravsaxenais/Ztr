namespace Business.GitRepository.Interfaces
{
    using Business.Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Module service.
    /// </summary>
    public interface IModuleServiceManager
    {
        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<List<ModuleReadModel>> GetAllModulesAsync(string firmwareVersion, string deviceType);

        /// <summary>
        /// Gets the tags earlier than this tag.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns></returns>
        Task <List<string>> GetTagsEarlierThanThisTagAsync(string firmwareVersion);

        /// <summary>
        /// Gets the default toml file content asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<string> GetDefaultTomlFileContentAsync(string firmwareVersion, string deviceType);

        /// <summary>
        /// Gets the proto files.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        string GetProtoFiles(ModuleReadModel module);

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitRepoAsync();
    }
}
