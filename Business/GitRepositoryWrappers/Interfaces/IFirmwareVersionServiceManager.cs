namespace Business.GitRepositoryWrappers.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Git repo wrapper for devices.
    /// </summary>
    public interface IFirmwareVersionServiceManager
    {
        /// <summary>
        /// Gets all firmware versions asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllFirmwareVersionsAsync(string deviceType);
    }
}
