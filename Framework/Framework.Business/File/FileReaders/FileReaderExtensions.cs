namespace ZTR.Framework.Business.File.FileReaders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;

    public static class FileReaderExtensions
    {
        private static readonly HashSet<char> InvalidCharacters = new HashSet<char>(Path.GetInvalidPathChars());

        /// <summary>
        /// Normalizes a given folder path by replacing all backslashes with forward slashes, as well as ensuring there is a trailing slash.
        /// Windows understands both forward and backslashes, but Linux only recognizes forward slashes.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path.</returns>
        public static string NormalizeFolderPath(string path)
        {
            EnsureArg.IsNotNullOrWhiteSpace(path, nameof(path));

            var normalizedPath = path.Replace(@"\", "/", StringComparison.InvariantCultureIgnoreCase);
            var hasTrailingSlash = normalizedPath.EndsWith("/", StringComparison.InvariantCultureIgnoreCase);
            if (!hasTrailingSlash)
            {
                normalizedPath += '/';
            }

            return normalizedPath;
        }

        /// <summary>
        /// Safely combines a folder path with a file name. Slashes are converted to forward slashes.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The full path.</returns>
        public static string ToSafeFullPath(string folderPath, string fileName)
        {
            var normalizedRemoteFolderPath = NormalizeFolderPath(folderPath);
            return Path.Combine(normalizedRemoteFolderPath, fileName);
        }

        /// <summary>
        /// Combines the path from application root.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string CombinePathFromAppRoot(string path)
        {
            string loaderPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            if (!string.IsNullOrEmpty(loaderPath)

                && loaderPath[loaderPath.Length - 1] != Path.DirectorySeparatorChar
                && loaderPath[loaderPath.Length - 1] != Path.AltDirectorySeparatorChar)
            {
                loaderPath += Path.DirectorySeparatorChar;
            }
            if (loaderPath.StartsWith(@"file:\"))
            {
                loaderPath = loaderPath[6..];
            }
            return Path.Combine(Path.GetDirectoryName(loaderPath), path);
        }

        /// <summary>
        /// Ensures the correct file system path.
        /// </summary>
        /// <param name="path">The path to file or directory.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string, contains only white space, or contains invalid characters as defined in <see cref="Path.GetInvalidPathChars"/></exception>
        /// <remarks>Throws an exception if <paramref name="path"/> is not a correct file system path, otherwise no.</remarks>
        public static void EnsureCorrectFileSystemPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException($"{nameof(path)} is null.", nameof(path));

            if (string.IsNullOrWhiteSpace(path) || path.Any(pc => InvalidCharacters.Contains(pc)))
            {
                var message =
                    $"{nameof(path)} is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.";
                throw new ArgumentException(message, nameof(path));
            }
        }

        private static async Task<string> ReadAllTextAsync(string path, Encoding encoding)
        {
            EnsureArg.IsNotNull(encoding, nameof(encoding));

            const int fileBufferSize = 4096;

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, fileBufferSize, true);
            using var reader = new StreamReader(fileStream, encoding);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        public static async Task<string> ReadAllTextAsync(string fileName, string folderPath, Encoding encoding)
        {
            EnsureCorrectFileSystemPath(Path.Combine(folderPath + fileName));
            var safeFullPath = ToSafeFullPath(folderPath, fileName);

            return await ReadAllTextAsync(safeFullPath, encoding);
        }

        public static async Task<string> ReadAllTextAsync(string fileName, string folderPath)
        {
            var safeFullPath = ToSafeFullPath(folderPath, fileName);

            return await ReadAllTextAsync(safeFullPath, Encoding.UTF8);
        }

        public static List<string> GetDirectories(string path, string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return GetDirectories(path, searchPattern).ToList();
            }

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var temp = 0; temp < directories.Count; temp++)
            {
                directories.AddRange(GetDirectories(directories[temp], searchPattern));
            }

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern = "*")
        {
            var directoryNames = new List<string>();

            foreach (var directory in Directory.GetDirectories(path, searchPattern))
            {
                var directoryInfo = new DirectoryInfo(directory);
                directoryNames.Add(directoryInfo.Name);
            }

            return directoryNames;
        }

        public static string GetSubDirectoryPath(string parentFolder, string folderNameToSearch, string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var subDirectoryList = new List<string>(Directory.EnumerateDirectories(parentFolder, searchPattern, searchOption));

            foreach (var directory in subDirectoryList)
            {
                var subDirectoryFolder = directory[(directory.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
                if (string.Equals(folderNameToSearch, subDirectoryFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return directory;
                }
            }

            return string.Empty;
        }
    }
}
