namespace ZTR.Framework.Business.File
{
    /// <summary>
    /// ExportFileResultModel
    /// </summary>
    public class ExportFileResultModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportFileResultModel"/> class.
        /// Empty constructor for serialization.
        /// </summary>
        public ExportFileResultModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportFileResultModel"/> class.
        /// </summary>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="data">The data.</param>
        /// <param name="fileName">Name of the file.</param>
        public ExportFileResultModel(string mimeType, byte[] data, string fileName)
        {
            MimeType = mimeType;
            FileName = fileName;
            Data = data;
        }

        /// <summary>
        /// Gets or sets the fileName.
        /// </summary>
        /// <value>
        /// The fileName.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the mimeType.
        /// </summary>
        /// <value>
        /// The mimeType.
        /// </value>
        public string MimeType { get; set; }
    }
}
