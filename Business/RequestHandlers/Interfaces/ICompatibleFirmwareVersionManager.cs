namespace Business.RequestHandlers.Interfaces
{
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// This Manager returns a list of all compatible firmware versions.
    /// </summary>
    public interface ICompatibleFirmwareVersionManager
    {
        /// <summary>
        /// Generates the configuration toml model asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns>ApiResponse - success: true/false. data: array of compatible firmware versions. </returns>
        Task<ApiResponse> GetCompatibleFirmwareVersionsAsync(string firmwareVersion);
    }
}
