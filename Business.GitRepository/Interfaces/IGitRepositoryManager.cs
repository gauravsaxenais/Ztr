namespace Business.GitRepository.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business.File;
    using ZTR.Framework.Configuration;

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
        /// Clones the repository asynchronous.
        /// </summary>
        /// <returns></returns>
        Task InitRepositoryAsync();

        /// <summary>
        /// Loads the tag names asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAllTagNamesAsync();

        /// <summary>
        /// Gets the tags earlier than this tag asynchronous.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
        Task<List<string>> GetTagsEarlierThanThisTagAsync(string tagName);

        /// <summary>
        /// Gets the file data from tag asynchronous.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        Task<IEnumerable<ExportFileResultModel>> GetFileDataFromTagAsync(string tag, string fileName);
    }
}
