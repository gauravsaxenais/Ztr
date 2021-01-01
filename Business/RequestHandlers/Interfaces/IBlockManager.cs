namespace Business.RequestHandlers.Interfaces
{
    using Business.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// This interface maps with BlockManager
    /// and responsible for parsing toml files.
    /// </summary>
    public interface IBlockManager
    {
        /// <summary>
        /// Parses the toml files asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<object> GetBlocksAsObjectAsync();

        /// <summary>
        /// Gets the list of blocks.
        /// </summary>
        /// <returns></returns>
        Task<List<BlockJsonModel>> GetListOfBlocksAsync();
    }
}
