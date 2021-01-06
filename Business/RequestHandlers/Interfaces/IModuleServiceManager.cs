namespace Business.RequestHandlers.Interfaces
{
    using System.IO;
    using Models;
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
        /// Gets the tags earlier than this tag asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns></returns>
        Task<List<string>> GetTagsEarlierThanThisTagAsync(string firmwareVersion);

        /// <summary>
        /// Gets the default content of the toml file.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<string> GetDefaultTomlFileContent(string firmwareVersion, string deviceType);

        /// <summary>
        /// Gets the proto files.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        string GetProtoFiles(ModuleReadModel module);

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllFirmwareVersionsAsync();

        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllDevicesAsync();

        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        IEnumerable<FileInfo> GetAllBlockFiles();
    }
}
