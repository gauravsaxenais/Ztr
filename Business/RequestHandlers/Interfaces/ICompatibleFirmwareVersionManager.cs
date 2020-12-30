namespace Business.RequestHandlers.Interfaces
{
    using Business.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// This Manager returns a list of all compatible firmware versions.
    /// </summary>
    public interface ICompatibleFirmwareVersionManager
    {
        /// <summary>
        /// Gets the compatible firmware versions asynchronous.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        Task<ApiResponse> GetCompatibleFirmwareVersionsAsync(CompatibleFirmwareVersionReadModel module);
    }
}
