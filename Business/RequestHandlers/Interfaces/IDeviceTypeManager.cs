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
        /// Gets the firmware git URL asynchronous.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<string> GetFirmwareGitUrlAsync(string deviceType);
    }
}
