namespace Business.RequestHandlers.Interfaces
{
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

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
        Task<ApiResponse> GetAllFirmwareVersionsAsync(string deviceType);
    }
}
