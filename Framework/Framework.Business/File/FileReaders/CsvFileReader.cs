namespace ZTR.Framework.Business.File.FileReaders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using CsvHelper.Configuration;

    /// <summary>
    /// A helper class for reading files
    /// </summary>
    public static class CsvFileReader
    {
        /// <summary>
        /// Extension for CSV files.
        /// </summary>
        public const string CsvFileExtension = ".csv";

        public static string GetLeafNode(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            var normalizedFolderPath = FileReaderExtensions.NormalizeFolderPath(path);
            var lastSlashIndex = normalizedFolderPath.LastIndexOf('/');
            var newFolderPath = lastSlashIndex == normalizedFolderPath.Length - 1
                ? normalizedFolderPath.Substring(0, lastSlashIndex)
                : normalizedFolderPath;
            lastSlashIndex = newFolderPath.LastIndexOf('/');
            return lastSlashIndex == -1 ? newFolderPath : newFolderPath[(lastSlashIndex + 1)..];
        }

        public static string GetProviderNode(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            var normalizedFolderPath = FileReaderExtensions.NormalizeFolderPath(path);
            int lastSlashIndex = normalizedFolderPath.LastIndexOf('/');
            int secondLastSlashIndex = lastSlashIndex > 0 ? normalizedFolderPath.LastIndexOf('/', lastSlashIndex - 1) : -1;
            return GetLeafNode(normalizedFolderPath.Substring(0, secondLastSlashIndex));
        }

        /// <summary>
        /// Safely combines a folder path with a file name. Slashes are converted to forward slashes.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The full path.</returns>
        public static string ToSafeFullPath(string folderPath, string fileName)
        {
            var normalizedRemoteFolderPath = FileReaderExtensions.NormalizeFolderPath(folderPath);
            return Path.Combine(normalizedRemoteFolderPath, fileName);
        }

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <typeparam name="T">type T</typeparam>
        /// <param name="remoteFolderPath">The remote folder path.</param>
        /// <param name="remoteFileName">Name of the remote file.</param>
        /// <param name="csvConfiguration">The CSV configuration.</param>
        /// <returns>
        /// IEnumerable of type T
        /// </returns>
        public static IEnumerable<T> ReadFile<T>(string remoteFolderPath, string remoteFileName, CsvConfiguration csvConfiguration)
        {
            ICollection<T> rawData;

            var safeFullPath = ToSafeFullPath(remoteFolderPath, remoteFileName);
            using (var reader = new StreamReader(safeFullPath))
            {
                using var csv = new CsvReader(reader, csvConfiguration);
                rawData = csv.GetRecords<T>().ToList();
            }

            return rawData;
        }

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <typeparam name="T">type T</typeparam>
        /// <param name="remoteFolderPath">The remote folder path.</param>
        /// <param name="remoteFileName">Name of the remote file.</param>
        /// <param name="fileDelimiter">The file delimiter.</param>
        /// <param name="fileDataHasHeader">if set to <c>true</c> [file data has header].</param>
        /// <returns>
        /// IEnumerable of type T
        /// </returns>
        public static IEnumerable<T> ReadFile<T>(string remoteFolderPath, string remoteFileName, string fileDelimiter, bool fileDataHasHeader)
        {
            ICollection<T> rawData;

            var safeFullPath = ToSafeFullPath(remoteFolderPath, remoteFileName);
            using (var reader = new StreamReader(safeFullPath))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = fileDelimiter,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    HasHeaderRecord = fileDataHasHeader,
                    TrimOptions = TrimOptions.Trim
                };

                using var csv = new CsvReader(reader, csvConfig);
                rawData = csv.GetRecords<T>().ToList();
            }

            return rawData;
        }

        /// <summary>
        /// Creates the CSV configuration.
        /// </summary>
        /// <param name="fileDelimiter">The file delimiter.</param>
        /// <param name="fileDataHasHeader">if set to <c>true</c> [file data has header].</param>
        /// <returns>CsvConfiguration</returns>
        private static CsvConfiguration CreateCsvConfiguration(string fileDelimiter, bool fileDataHasHeader)
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = context => throw new InvalidOperationException($"Could not parse this record from the CSV: {context.RawRecord}"),
                Delimiter = fileDelimiter,
                MissingFieldFound = null,
                HeaderValidated = null,
                HasHeaderRecord = fileDataHasHeader,
                TrimOptions = TrimOptions.Trim
            };
        }
    }
}
