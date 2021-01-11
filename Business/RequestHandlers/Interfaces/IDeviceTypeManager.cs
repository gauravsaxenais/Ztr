namespace Business.RequestHandlers.Interfaces
{
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Gets all the devices e.g. M3, M7, M9 etc.
    /// </summary>
    public interface IDeviceTypeManager
    {
        /// <summary>
        /// Gets all devices asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse> GetAllDevicesAsync();
    }
}
