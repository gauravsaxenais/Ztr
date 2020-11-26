namespace Business.RequestHandlers.Interfaces
{
    using System.Threading.Tasks;

    public interface IDefaultValueManager
    {
        Task<string> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType);
    }
}
