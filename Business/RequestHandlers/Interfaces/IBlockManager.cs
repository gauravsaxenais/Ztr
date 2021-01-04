namespace Business.RequestHandlers.Interfaces
{
    using ZTR.Framework.Business;
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
        /// Parses the toml files asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse> GetBlocksAsObjectAsync();

        /// <summary>
        /// Gets the list of blocks asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<BlockJsonModel>> GetListOfBlocksAsync();
    }
}
