namespace Business.RequestHandlers.Interfaces
{
    using Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
        Task<IEnumerable<string>> GetCompatibleFirmwareVersionsAsync(CompatibleFirmwareVersionReadModel module);
    }
}
