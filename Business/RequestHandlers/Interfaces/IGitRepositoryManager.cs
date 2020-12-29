namespace Business.RequestHandlers.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business.File;

    /// <summary>
    /// Git repo manager - clones a repo and gets all the tags.
    /// </summary>
    public interface IGitRepositoryManager
    {
        /// <summary>
        /// Sets the connection options.
        /// </summary>
        /// <param name="gitConnection">The git connection.</param>
        void SetConnectionOptions(GitConnectionOptions gitConnection);

        /// <summary>
        /// Gets the connection options.
        /// </summary>
        /// <returns></returns>
        GitConnectionOptions GetConnectionOptions();

        /// <summary>
        /// Clones the repository asynchronous.
        /// </summary>
        /// <returns></returns>
        Task CloneRepositoryAsync();

        /// <summary>
        /// Loads the tag names asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<string[]> GetAllTagsAsync();

        /// <summary>
        /// Gets the file data from tag asynchronous.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        Task<IEnumerable<ExportFileResultModel>> GetFileDataFromTagAsync(string tag, string fileName);
    }
}
