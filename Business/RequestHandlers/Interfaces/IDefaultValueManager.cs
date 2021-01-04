using ZTR.Framework.Business;

namespace Business.RequestHandlers.Interfaces
{
    using Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Gets default values for all the modules
    /// their names and uuid information.
    /// </summary>
    public interface IDefaultValueManager
    {
        /// <summary>
        /// Gets the default values all modules asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<ApiResponse> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType);
        
        /// <summary>
        /// Merges the values with modules asynchronous.
        /// </summary>
        /// <param name="defaultValueFromTomlFile">The default value from toml file.</param>
        /// <param name="listOfModules">The list of modules.</param>
        /// <returns></returns>
        Task MergeValuesWithModulesAsync(string defaultValueFromTomlFile, IEnumerable<ModuleReadModel> listOfModules);
    }
}
