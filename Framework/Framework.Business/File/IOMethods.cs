namespace ZTR.Framework.Business.File
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public static class IOmethods
    {
        #region Private Methods
        private static long GetDirectorySize(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            return DirectorySize(directoryInfo);
        }

        private static DirectoryInfo GetDirectoryInfo(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            return directoryInfo;
        }

        private static FileInfo GetFileInfo(string path)
        {
            FileInfo fileInfo = new FileInfo(path);

            return fileInfo;
        }

        private static long DirectorySize(DirectoryInfo dirInfo)
        {
            long size = 0;

            // Add file sizes.
            FileInfo[] fis = dirInfo.GetFiles();
            foreach (FileInfo fileInfo in fis)
            {
                size += fileInfo.Length;
            }

            return (size);
        }
        private static bool AppearIdentical(string source, string mirror)
        {
            long sourceSize = GetDirectorySize(source);

            long mirrorSize = GetDirectorySize(mirror);

            if (sourceSize == mirrorSize) return true;
            return false;
        }
        #endregion

        #region Public Methods
        public static bool DirectoryExists(string path)
        {
            if (Directory.Exists(path)) return true;
            return false;
        }
        public static bool HasFolderPermission(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, path);
            permissionSet.AddPermission(writePermission);

            if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
                return true;

            return false;
        }
        public static void CopyFileToDestination(string path, string source, string mirror)
        {
            string target = path.Replace(source, mirror);

            //does destinations directory exist ?
            if (!DirectoryExists(mirror))
                Directory.CreateDirectory(mirror);

            //makes sure it is not the same file        
            if (source.Equals(mirror))
            {
                string message = "Unable to write file '" + source + "' on itself.";
                throw new IOException(message);
            }

            if (DirectoryExists(mirror) && !HasFolderPermission(mirror))
            {
                string message = "Unable to open file '" + mirror + "' for writing.";
                throw new IOException(message);
            }

            if (IsFileClosed(path))
                File.Copy(path, target, true);
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
                    Thread.Sleep(delay);
            }
            while (!fileClosed && retries > 0);

            return fileClosed;
        }

        #region Soft Rebuild
        public static void MirrorUpdate(string source, string mirror)
        {
            // check a basic folder size comparison
            if (AppearIdentical(source, mirror)) return;

            string[] ioData = Directory.GetFileSystemEntries(source);

            // Add missing folders and files
            foreach (string path in ioData)
            {
                if (File.Exists(path))
                    // This path is a file
                    CopyFileToDestination(path, source, mirror);
            }
        }
        #endregion

        public static void DeleteAll(string path)
        {
            if (Directory.Exists(path))
            {
                String[] Files;
                Files = Directory.GetFileSystemEntries(path);
                foreach (string element in Files)
                {
                    if (Directory.Exists(element))
                        Directory.Delete(element, true);
                    else
                        File.Delete(element);
                }
            }
        }

        public static bool DeleteFile(string path)
        {
            if (IsFileClosed(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }

        public static int GetFileCount(string path)
        {
            string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);

            int length = files.Length;
            //files = null;
            return length;
        }
        #endregion
    }
}
