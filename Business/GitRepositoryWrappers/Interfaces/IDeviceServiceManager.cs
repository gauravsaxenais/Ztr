namespace Business.GitRepositoryWrappers.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Git repo wrapper for devices.
    /// </summary>
    public interface IDeviceServiceManager
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
