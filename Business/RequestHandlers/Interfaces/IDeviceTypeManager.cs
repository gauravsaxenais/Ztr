namespace Business.RequestHandlers.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Gets all the devices e.g. M3, M7, M9 etc.
    /// </summary>
    public interface IDeviceTypeManager
    {
        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllDevicesAsync();

        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllFirmwareVersionsAsync();
    }
}
