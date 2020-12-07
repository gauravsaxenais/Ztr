namespace Business.RequestHandlers.Interfaces
{
    using Business.Models;
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

        /// <summary>
        /// Gets the model by name asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="names">The names.</param>
        /// <returns></returns>
        Task<IEnumerable<ModuleReadModel>> GetModelByNameAsync(string firmwareVersion, string deviceType, IEnumerable<string> names);

        /// <summary>
        /// Gets the module by name asynchronous.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="names">The names.</param>
        /// <returns></returns>
        Task<IEnumerable<ModuleReadModel>> GetModuleByNameAsync(string name, string firmwareVersion, string deviceType, params string[] names);
    }
}
