namespace ZTR.Framework.Business.File.FileReaders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Renci.SshNet;
    using Renci.SshNet.Sftp;
    using ZTR.Framework.Configuration.Ftp;

    public static class FileReaderExtensions
    {
        private readonly static HashSet<char> _invalidCharacters = new HashSet<char>(Path.GetInvalidPathChars());

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
        /// Uploads the file.
        /// </summary>
        /// <param name="ftpConnection">The FTP connection.</param>
        /// <param name="remoteFolderPath">The remote folder path.</param>
        /// <param name="remoteFileName">Name of the remote file.</param>
        /// <param name="file">The file.</param>
        public static void UploadFile(FtpConnection ftpConnection, string remoteFolderPath, string remoteFileName, byte[] file)
        {
            using var memoryStream = new MemoryStream();
            memoryStream.Write(file, 0, file.Length);

            var safeFullPath = ToSafeFullPath(remoteFolderPath, remoteFileName);
            using var sftp = ftpConnection.ToSftpClient();
            sftp.Connect();
            sftp.UploadFile(memoryStream, safeFullPath);
            sftp.Disconnect();
        }

        /// <summary>
        /// Delete the file.
        /// </summary>
        /// <param name="ftpConfiguration">The FTP connection.</param>
        /// <param name="fileName">The file.</param>
        public static void DeleteFile(FtpConfiguration ftpConfiguration, string fileName)
        {
            using var sftp = ftpConfiguration.ToSftpClient();
            sftp.Connect();
            var safeFullPath = ToSafeFullPath(ftpConfiguration.FolderPath, fileName);
            sftp.DeleteFile(safeFullPath);
            sftp.Disconnect();
        }

        /// <summary>
        /// Delete the file.
        /// </summary>
        /// <param name="ftpConnection">The FTP connection.</param>
        /// /// <param name="remoteFolderPath">The remote folder path.</param>
        /// <param name="fileName">The file.</param>
        public static void DeleteFile(FtpConnection ftpConnection, string remoteFolderPath, string fileName)
        {
            using var sftp = ftpConnection.ToSftpClient();
            sftp.Connect();
            var safeFullPath = ToSafeFullPath(remoteFolderPath, fileName);
            sftp.DeleteFile(safeFullPath);
            sftp.Disconnect();
        }

        public static IEnumerable<string> GetAllFileNames(FtpConfiguration ftpConfiguration, FileSortOrder fileSortOrder = FileSortOrder.FilenameDescending)
        {
            using var sftp = ftpConfiguration.ToSftpClient();
            sftp.Connect();
            var sftpFiles = GetFiles(sftp, ftpConfiguration, fileSortOrder);
            return sftpFiles.Select(item => item.Name);
        }

        /// <summary>
        /// Gets the list of directory.
        /// </summary>
        /// <param name="ftpConnection">The ftpconnection details</param>
        /// <param name="folderPath">The folderpath</param>
        /// <param name="domain">The domain from directory</param>
        /// <returns>IEnumarable of the directory</returns>
        public static IEnumerable<string> ListDirectory(FtpConnection ftpConnection, string folderPath, string domain = "")
        {
            EnsureArg.IsNotNull(ftpConnection, nameof(ftpConnection));
            EnsureArg.IsNotNullOrEmpty(folderPath, nameof(folderPath));
            using var sftp = ftpConnection.ToSftpClient();
            sftp.Connect();
            var directories = new List<string>();
            var normalizedFolderPath = NormalizeFolderPath(folderPath);
            ListDirectory(sftp, normalizedFolderPath, ref directories);

            return !string.IsNullOrEmpty(domain) ? directories.Where(item => item.EndsWith(domain, StringComparison.OrdinalIgnoreCase)) : directories;
        }

        /// <summary>
        /// Gets the list of directory.
        /// </summary>
        /// <param name="ftpConfiguration">The FTP configuration.</param>
        /// <param name="domain">The domain from directory</param>
        /// <returns>IEnumerable  of the directory</returns>
        public static IEnumerable<string> ListDirectory(FtpConfiguration ftpConfiguration, string domain = "")
        {
            EnsureArg.IsNotNull(ftpConfiguration, nameof(ftpConfiguration));
            EnsureArg.IsNotNullOrEmpty(ftpConfiguration.FolderPath, nameof(ftpConfiguration.FolderPath));
            using var sftp = ftpConfiguration.ToSftpClient();
            sftp.Connect();
            var directories = new List<string>();
            var normalizedFolderPath = NormalizeFolderPath(ftpConfiguration.FolderPath);

            ListDirectory(sftp, normalizedFolderPath, ref directories);
            return !string.IsNullOrEmpty(domain) ? directories.Where(item => item.EndsWith(domain, StringComparison.OrdinalIgnoreCase)) : directories;
        }

        /// <summary>
        /// Gets the list of directory
        /// </summary>
        /// <param name="sftpClient">The SFTP client.</param>
        /// <param name="folderPath">The folderpath</param>
        /// <param name="directories">The list of directory</param>
        public static void ListDirectory(SftpClient sftpClient, string folderPath, ref List<string> directories)
        {
            foreach (var directory in sftpClient.ListDirectory(folderPath).Where(ftpFile => ftpFile.IsDirectory && ftpFile.Name != "." && ftpFile.Name != ".."))
            {
                directories.Add(directory.FullName);
                if (directory.IsDirectory)
                {
                    ListDirectory(sftpClient, directory.FullName, ref directories);
                }
            }
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="sftpClient">The SFTP client.</param>
        /// <param name="ftpConfiguration">The FTP configuration.</param>
        /// <param name="fileSortOrder">The order to return the filenames in.</param>
        /// <returns>IEnumerable of SftpFile</returns>
        public static IEnumerable<SftpFile> GetFiles(SftpClient sftpClient, FtpConfiguration ftpConfiguration, FileSortOrder fileSortOrder = FileSortOrder.FilenameDescending)
        {
            IEnumerable<SftpFile> files;
            if (!string.IsNullOrEmpty(ftpConfiguration.FileNamePreFix) && ftpConfiguration.ExcludeFileList != null && ftpConfiguration.ExcludeFileList.Any())
            {
                files = sftpClient
                    .ListDirectory(NormalizeFolderPath(ftpConfiguration.FolderPath))
                    .Where(ftpFile => ftpFile.IsRegularFile
                                      && ftpFile.Name.StartsWith(ftpConfiguration.FileNamePreFix, StringComparison.InvariantCultureIgnoreCase)
                                      && ftpFile.Name.EndsWith(ftpConfiguration.FileExtension, StringComparison.OrdinalIgnoreCase)
                                      && !ftpConfiguration.ExcludeFileList.Contains(ftpFile.Name));
            }
            else if (string.IsNullOrEmpty(ftpConfiguration.FileNamePreFix) && ftpConfiguration.ExcludeFileList != null && ftpConfiguration.ExcludeFileList.Any())
            {
                files = sftpClient
                    .ListDirectory(NormalizeFolderPath(ftpConfiguration.FolderPath))
                    .Where(ftpFile => ftpFile.IsRegularFile
                                      && ftpFile.Name.EndsWith(ftpConfiguration.FileExtension, StringComparison.OrdinalIgnoreCase)
                                      && !ftpConfiguration.ExcludeFileList.Contains(ftpFile.Name));
            }
            else if (!string.IsNullOrEmpty(ftpConfiguration.FileNamePreFix) && (ftpConfiguration.ExcludeFileList == null || !ftpConfiguration.ExcludeFileList.Any()))
            {
                files = sftpClient
                    .ListDirectory(NormalizeFolderPath(ftpConfiguration.FolderPath))
                    .Where(ftpFile => ftpFile.IsRegularFile
                                      && ftpFile.Name.StartsWith(ftpConfiguration.FileNamePreFix, StringComparison.InvariantCultureIgnoreCase)
                                      && ftpFile.Name.EndsWith(ftpConfiguration.FileExtension, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                files = sftpClient
                    .ListDirectory(NormalizeFolderPath(ftpConfiguration.FolderPath))
                    .Where(ftpFile => ftpFile.IsRegularFile
                                      && ftpFile.Name.EndsWith(ftpConfiguration.FileExtension, StringComparison.OrdinalIgnoreCase));
            }

            return fileSortOrder switch
            {
                FileSortOrder.FilenameAscending => files.OrderBy(x => x.Name),
                _ => files.OrderByDescending(x => x.Name)
            };
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

            if (string.IsNullOrWhiteSpace(path) || path.Any(pc => _invalidCharacters.Contains(pc)))
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

        public static bool IsFileClosed(string filepath)
        {
            bool fileClosed = false;
            int retries = 20;
            const int delay = 400; // Max time spent here = retries*delay milliseconds

            if (!File.Exists(filepath))
                return false;

            do
            {
                try
                {
                    // Attempts to open then close the file in RW mode, denying other users to place any locks.
                    FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fs.Close();
                    fileClosed = true; // success
                }
                catch (IOException) { }

                retries--;

                if (!fileClosed)
                {
                    Thread.Sleep(delay);
                }
            }
            while (!fileClosed && retries > 0);

            return fileClosed;
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
                var subDirectoryFolder = directory.Substring(directory.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                if (string.Equals(folderNameToSearch, subDirectoryFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return directory;
                }
            }

            return string.Empty;
        }
    }
}
