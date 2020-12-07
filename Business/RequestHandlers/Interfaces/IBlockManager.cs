namespace Business.RequestHandlers.Interfaces
{
    using Newtonsoft.Json;
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
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="parserType">Type of the parser.</param>
        /// <returns></returns>
        Task<string> ParseTomlFilesAsync(string firmwareVersion, string deviceType, string parserType);
    }
}
