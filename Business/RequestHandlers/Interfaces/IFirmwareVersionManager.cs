namespace Business.RequestHandlers.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Gets list of all firmware versions.
    /// </summary>
    public interface IFirmwareVersionManager
    {
        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllFirmwareVersionsAsync(string deviceType);
    }
}
