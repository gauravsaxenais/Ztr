namespace Business.GitRepository.Interfaces
{
    using Business.Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Module service.
    /// </summary>
    public interface IModuleServiceManager
    {
        Task<List<ModuleReadModel>> GetAllModulesAsync(string firmwareVersion, string deviceType, string moduleFilePath,
            string deviceTomlFilePath, string metaTomlFilePath);

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
        /// <param name="defaultFilePath">The default file path.</param>
        /// <returns></returns>
        Task<string> GetDefaultTomlFileContentAsync(string firmwareVersion, string deviceType, string defaultFilePath);

        /// <summary>
        /// Gets the proto files.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="moduleFilePath">The module file path.</param>
        /// <returns></returns>
        string GetProtoFiles(ModuleReadModel module, string moduleFilePath);

        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        /// <param name="connectionOptions">The connection options.</param>
        void SetGitRepoConnection(GitConnectionOptions connectionOptions);
    }
}
