namespace Business.RequestHandlers.Interfaces
{
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    public interface IBlockManager
    {
        Task<string> ParseTomlFilesAsync(string firmwareVersion, string deviceType, string parserType);
    }
}
