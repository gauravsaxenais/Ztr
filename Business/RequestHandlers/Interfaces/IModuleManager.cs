namespace Business.RequestHandlers.Interfaces
{
    using Business.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// get list of all the modules, their name and uuid information.
    /// </summary>
    public interface IModuleManager
    {
        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<ApiResponse> GetAllModulesAsync(string firmwareVersion, string deviceType);

        /// <summary>
        /// Gets the list of modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<List<ModuleReadModel>> GetListOfModulesAsync(string firmwareVersion, string deviceType);
    }
}
