namespace ZTR.M7Config.Business.RequestHandlers.Interfaces
{
    using Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// This interface maps with BlockManager
    /// and responsible for parsing toml files.
    /// </summary>
    public interface IBlockManager
    {
        /// <summary>
        /// Gets the blocks asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<object> GetBlocksAsync(string firmwareVersion, string deviceType);

        /// <summary>
        /// Gets the list of blocks asynchronous.
        /// </summary>
        /// <param name="configTomlFileContent">Content of the configuration toml file.</param>
        /// <returns></returns>
        Task<List<BlockJsonModel>> GetBlocksFromFileAsync(string configTomlFileContent);
    }
}
