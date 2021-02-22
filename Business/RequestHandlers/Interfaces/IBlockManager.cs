namespace Business.RequestHandlers.Interfaces
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
        /// <returns></returns>
        Task<object> GetBlocksAsync(string firmwareVersion);

        /// <summary>
        /// Gets the list of blocks asynchronous.
        /// </summary>
        /// <param name="configTomlFileContent">Content of the configuration toml file.</param>
        /// <returns></returns>
        Task<List<BlockJsonModel>> GetBlocksFromFileAsync(string configTomlFileContent);
    }
}
