namespace Business.RequestHandlers.Interfaces
{
    using Business.Models.ConfigModels;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDefaultValueManager
    {
        Task<IEnumerable<ModuleReadModel>> GetAllModulesAsync();
        Task<IEnumerable<string>> GetAllUuidsAsync();
        Task<IEnumerable<ModuleReadModel>> GetModuleByNameAsync(IEnumerable<string> names);
        Task<IEnumerable<ModuleReadModel>> GetModuleByNameAsync(string name, params string[] names);
        Task<ConfigurationReadModel> GetNetworkInformationAsync();
    }
}
