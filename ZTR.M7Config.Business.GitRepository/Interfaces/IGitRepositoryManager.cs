namespace ZTR.M7Config.Business.GitRepository.Interfaces
{
    using global::ZTR.Framework.Business.File;
    using global::ZTR.Framework.Configuration;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    /// <summary>
    /// Git repo manager - clones a repo and gets all the tags.
    /// </summary>
    public interface IGitRepositoryManager
    {
        /// <summary>
        /// Sets the connection options.
        /// </summary>
        /// <param name="gitConnection">The git connection.</param>
        /// <param name="path">The path.</param>
        void SetConnectionOptions(GitConnectionOptions gitConnection, string path = "");

        /// <summary>
        /// Clones the repository asynchronous.
        /// </summary>
        /// <returns></returns>
        Task InitRepositoryAsync();

        /// <summary>
        /// Gets all tag names asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<ConcurrentDictionary<string, DateTimeOffset>> GetAllTagNamesAsync();

        /// <summary>
        /// Gets the file data from tag asynchronous.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        Task<ExportFileResultModel> GetFileDataFromTagAsync(string tag, string pathToFile);

        /// <summary>
        /// Determines whether [is file changed between tags] [the specified tag from].
        /// </summary>
        /// <param name="tagFrom">The tag from.</param>
        /// <param name="tagTo">The tag to.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        ///   <c>true</c> if [is file changed between tags] [the specified tag from]; otherwise, <c>false</c>.
        /// </returns>
        bool IsFileChangedBetweenTags(string tagFrom, string tagTo, string filePath);

        /// <summary>
        /// Determines whether [is folder present in tag] [the specified tag].
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        Task<bool> IsFolderPresentInTag(string tag, string folderName);
    }
}
