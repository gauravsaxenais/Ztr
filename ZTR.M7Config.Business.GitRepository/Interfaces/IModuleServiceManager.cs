namespace ZTR.M7Config.Business.GitRepository.Interfaces
{
    using ZTR.M7Config.Business.Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Module service.
    /// </summary>
    public interface IModuleServiceManager
    {
        /// <summary>
        /// Sets the connection.
        /// </summary>
        void SetConnection();

        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="listOfModules">The list of modules.</param>
        /// <returns></returns>
        Task UpdateMetaTomlAsync(List<ModuleReadModel> listOfModules);

        /// <summary>
        /// Gets the proto files.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        string GetProtoFiles(ModuleReadModel module);

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneGitRepoAsync();
    }
}
