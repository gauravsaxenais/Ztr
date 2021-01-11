namespace Business.GitRepositoryWrappers.Interfaces
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Block Service Manager.
    /// </summary>
    public interface IBlockServiceManager
    {
        /// <summary>
        /// Gets all block files.
        /// </summary>
        /// <returns></returns>
        IEnumerable<FileInfo> GetAllBlockFiles();
    }
}
