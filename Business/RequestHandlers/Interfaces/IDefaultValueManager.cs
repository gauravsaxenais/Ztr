namespace Business.RequestHandlers.Interfaces
{
    using Business.Models.ConfigModels;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDefaultValueManager
    {
        Task<IEnumerable<ModuleReadModel>> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType);
    }
}
