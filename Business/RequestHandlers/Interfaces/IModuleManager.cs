namespace Business.RequestHandlers.Interfaces
{
    using Business.Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
        Task<IEnumerable<ModuleReadModel>> GetAllModulesAsync(string firmwareVersion, string deviceType);
    }
}
