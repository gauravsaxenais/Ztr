namespace ZTR.Framework.Business.File
{
    using System.Collections.Generic;
    using EnsureThat;

    /// <summary>
    /// A class for parsing files into models.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public class ParsedFile<TModel>
    {
        public ParsedFile(string filename, IEnumerable<TModel> models, string folderPath)
        {
            EnsureArg.IsNotNullOrWhiteSpace(filename, nameof(filename));
            EnsureArg.IsNotNull(models, nameof(models));
            EnsureArg.IsNotNullOrWhiteSpace(folderPath, nameof(folderPath));
            Filename = filename;
            Models = models;
            FolderPath = folderPath;
        }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public string Filename { get; }

        /// <summary>
        /// Gets the models.
        /// </summary>
        /// <value>
        /// The models.
        /// </value>
        public IEnumerable<TModel> Models { get; }

        /// <summary>
        /// Gets the folder path.
        /// </summary>
        /// <value>
        /// The folder path.
        /// </value>
        public string FolderPath { get; }
    }
}
