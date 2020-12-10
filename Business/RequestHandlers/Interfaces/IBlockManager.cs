namespace Business.RequestHandlers.Interfaces
{
    using Business.Models;
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
        Task<object> ParseTomlFilesAsync();
    }
}
