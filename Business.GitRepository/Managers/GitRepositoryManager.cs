namespace Business.GitRepository.Managers
{
    using EnsureThat;
    using Interfaces;
    using LibGit2Sharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File;
    using ZTR.Framework.Business.File.FileReaders;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration;
    using Blob = LibGit2Sharp.Blob;

    /// <summary>
    /// Git repo manager is responsible for cloning a git repo,
    /// gets all the tags.
    /// </summary>
    /// <seealso cref="IGitRepositoryManager" />
    public sealed class GitRepositoryManager : IGitRepositoryManager, IDisposable
    {
        #region Fields
        private GitConnectionOptions _gitConnection;
        private const string GitFolder = ".git";
        private const string TextMimeType = "text/plain";
        private Repository _repository;
        private UsernamePasswordCredentials _userNamePasswordCredentials;
        private readonly object _syncRoot = new object();
        #endregion

        #region Public methods
        /// <summary>
        /// Sets the connection options.
        /// </summary>
        /// <param name="gitConnection">The git connection.</param>
        public void SetConnectionOptions(GitConnectionOptions gitConnection)
        {
            EnsureArg.IsNotNull(gitConnection);
            EnsureArg.IsNotEmptyOrWhiteSpace(gitConnection.GitLocalFolder);
            EnsureArg.IsNotEmptyOrWhiteSpace(gitConnection.GitRemoteLocation);

            _gitConnection = gitConnection;

            _userNamePasswordCredentials = new UsernamePasswordCredentials()
            {
                Username = gitConnection.UserName,
                Password = gitConnection.Password
            };
        }

        /// <summary>
        /// Clones the repository asynchronous.
        /// </summary>
        /// <exception cref="Exception">
        /// </exception>/
        public async Task InitRepositoryAsync()
        {
            lock (_syncRoot)
            {
                // clone only when there is a change.
                // and local repository matches remote repo details
                if (IsExistsLocalRepositoryDirectory() && IsLocalRepositorySameAsRemote())
                {
                    try
                    {
                        GetLatestFromRepository();
                    }
                    catch (LibGit2SharpException)
                    {
                        ActionOnRepositoryWithoutHttps(GetLatestFromRepository);
                    }
                }
                else
                {
                    try
                    {
                        if (Directory.Exists(_gitConnection.GitLocalFolder))
                        {
                            DeleteDirectory(_gitConnection.GitLocalFolder);
                        }
                        Directory.CreateDirectory(_gitConnection.GitLocalFolder);
                        CloneRepository();
                    }
                    catch (LibGit2SharpException)
                    {
                        ActionOnRepositoryWithoutHttps(CloneRepository);
                    }
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets all tag names asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, DateTimeOffset>> GetAllTagNamesAsync()
        {
            var tags = new Dictionary<string, DateTimeOffset>();
            _repository = new Repository(_gitConnection.GitLocalFolder);

            // Add new tags.
            foreach (var tag in _repository.Tags)
            {
                var peeledTarget = tag.PeeledTarget;

                if (peeledTarget is Commit)
                {
                    // We're not interested by Tags pointing at Blobs or Trees
                    // only interested in tags for a commit.
                    tags.Add(tag.FriendlyName, ((Commit)tag.PeeledTarget).Author.When);
                }
            }

            return await Task.FromResult(tags);
        }

        /// <summary>
        /// This method returns a file data as string from a particular tag.
        /// If a tag is "1.0.7", then the method returns data from all the files
        /// for a particular tag.
        /// </summary>
        /// <param name="tag">Tag like 1.0.7 etc. in github.</param>
        /// <param name="pathToFile">path to file</param>
        /// <returns></returns>
        public async Task<ExportFileResultModel> GetFileDataFromTagAsync(string tag, string pathToFile)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(tag, nameof(tag));
            EnsureArg.IsNotEmptyOrWhiteSpace(pathToFile, nameof(pathToFile));

            var fileContent = await GetFileContentFromTag(tag, pathToFile).ConfigureAwait(false);
            return fileContent;
        }

        /// <summary>
        /// Determines whether [is folder present in tag] [the specified tag].
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        public async Task<bool> IsFolderPresentInTag(string tag, string folderName)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(tag, nameof(tag));
            EnsureArg.IsNotEmptyOrWhiteSpace(folderName, nameof(folderName));

            folderName = FileReaderExtensions.NormalizeFolderPath(folderName);

            var isPresent = await IsFileFolderPresentInTag(tag, folderName).ConfigureAwait(false);

            return isPresent;
        }

        /// <summary>
        /// Determines whether [is file changed between tags] [the specified tag from].
        /// </summary>
        /// <param name="tagFrom">The tag from.</param>
        /// <param name="tagTo">The tag to.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        ///   <c>true</c> if [is file changed between tags] [the specified tag from]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFileChangedBetweenTags(string tagFrom, string tagTo, string filePath)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(tagFrom, nameof(tagFrom));
            EnsureArg.IsNotEmptyOrWhiteSpace(tagTo, nameof(tagTo));
            EnsureArg.IsNotEmptyOrWhiteSpace(filePath, nameof(filePath));

            filePath = FileReaderExtensions.NormalizeFolderPath(filePath);
            Tag toTag = _repository.Tags[tagTo];
            Tag fromTag = _repository.Tags[tagFrom];

            var commitFrom = _repository.Lookup<Commit>(fromTag.Target.Sha);
            var commitTo = _repository.Lookup<Commit>(toTag.Target.Sha);

            IEnumerable<TreeEntryChanges> modifiedChanges = _repository.Diff.Compare<TreeChanges>(commitFrom.Tree, commitTo.Tree).Modified;
            var modifiedPaths = modifiedChanges.Select(entry => FileReaderExtensions.NormalizeFolderPath(entry.Path)).ToList();
            var result = modifiedPaths.Any(temp => temp.Contains(filePath, StringComparison.OrdinalIgnoreCase));

            return result;
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _repository?.Dispose();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Checks a GIT tree to see if a file exists
        /// </summary>
        /// <param name="tree">The GIT tree</param>
        /// <param name="filename">The file name</param>
        /// <returns>true if file exists</returns>
        private bool IsTreeContainsFile(Tree tree, string filename)
        {
            if (tree.Any(x => FileReaderExtensions.NormalizeFolderPath(x.Path) == FileReaderExtensions.NormalizeFolderPath(filename)))
            {
                return true;
            }
            else
            {
                foreach (Tree branch in tree.Where(x => x.TargetType == TreeEntryTargetType.Tree).Select(x => x.Target as Tree))
                {
                    if (IsTreeContainsFile(branch, filename))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Gets the file content from tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="pathToFile">The path to file.</param>
        /// <returns></returns>
        /// <exception cref="CustomArgumentException">Firmware version is not valid/present in the system.</exception>
        private async Task<ExportFileResultModel> GetFileContentFromTag(string tag, string pathToFile)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(tag);

            try
            {
                _repository = new Repository(_gitConnection.GitLocalFolder);
                var repoTag = _repository.Tags[tag];

                if (repoTag == null)
                {
                    throw new CustomArgumentException("Firmware version is not valid/present in the system.");
                }

                ObjectId commitForTag = GetCommitForTag(repoTag);
                pathToFile = FileReaderExtensions.NormalizeFolderPath(pathToFile);

                // Let's enumerate all the reachable commits (similarly to `git log --all`)
                foreach (var commit in _repository.Commits.QueryBy(new CommitFilter
                { IncludeReachableFrom = commitForTag }))
                {
                    if (commit.Id == commitForTag)
                    {
                        return GetContentOfFile(commit.Tree, pathToFile);
                    }
                }

                await Task.CompletedTask;
                return null;
            }
            catch (LibGit2SharpException)
            {
                throw;
            }
        }
        private async Task<bool> IsFileFolderPresentInTag(string tag, string path)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(tag);

            try
            {
                var listOfContentFiles = new List<ExportFileResultModel>();

                _repository = new Repository(_gitConnection.GitLocalFolder);
                var repoTag = _repository.Tags[tag];

                if (repoTag == null)
                {
                    throw new CustomArgumentException("Firmware version is not valid/present in the system.");
                }

                ObjectId commitForTag = GetCommitForTag(repoTag);

                // Let's enumerate all the reachable commits (similarly to `git log --all`)
                foreach (var commit in _repository.Commits.QueryBy(new CommitFilter
                { IncludeReachableFrom = commitForTag }))
                {
                    if (commit.Id == commitForTag)
                    {
                        return IsTreeContainsFile(commit.Tree, path);
                    }
                }

                await Task.CompletedTask;
                return false;
            }
            catch (LibGit2SharpException)
            {
                throw;
            }
        }
        private bool CertificateValidationCallback(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors errors) { return true; }
        private void CloneRepository()
        {
            var cloneOptions = new CloneOptions();
            cloneOptions.CertificateCheck += (certificate, valid, host) => true;

            cloneOptions.CredentialsProvider = (_url, _user, _cred) => new DefaultCredentials();
            cloneOptions.CredentialsProvider += (_url, _user, _cred) => _userNamePasswordCredentials;
            Repository.Clone(_gitConnection.GitRemoteLocation, _gitConnection.GitLocalFolder,
            cloneOptions);

            _repository = new Repository(_gitConnection.GitLocalFolder);
        }

        private void ActionOnRepositoryWithoutHttps(Action action)
        {
            SmartSubtransportRegistration<MockSmartSubtransport> registration = null;
            var scheme = "https";
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallback;
                registration = GlobalSettings.RegisterSmartSubtransport<MockSmartSubtransport>(scheme);
                action();
            }
            finally
            {
                GlobalSettings.UnregisterSmartSubtransport(registration);
                ServicePointManager.ServerCertificateValidationCallback -= CertificateValidationCallback;
            }
        }

        private void GetLatestFromRepository()
        {
            _repository = new Repository(_gitConnection.GitLocalFolder);
            var network = _repository.Network.Remotes.First();
            var refSpecs = new List<string>() { network.FetchRefSpecs.First().Specification };
            var fetchOptions = new FetchOptions { TagFetchMode = TagFetchMode.All };
            fetchOptions.CredentialsProvider = (_url, _user, _cred) => new DefaultCredentials();
            fetchOptions.CredentialsProvider += (_url, _user, _cred) => _userNamePasswordCredentials;

            _repository.Network.Fetch(network.Name, refSpecs, fetchOptions);
        }

        /// <summary>
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
        /// </summary>
        /// <param name="directory">The name of the directory to remove.</param>
        private static void DeleteDirectory(string directory)
        {
            var directoryInfos = new Stack<DirectoryInfo>();
            var root = new DirectoryInfo(directory);
            directoryInfos.Push(root);

            while (directoryInfos.Count > 0)
            {
                var fol = directoryInfos.Pop();
                fol.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);

                foreach (var d in fol.GetDirectories())
                {
                    directoryInfos.Push(d);
                }

                foreach (var f in fol.GetFiles())
                {
                    f.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                    f.Delete();
                }
            }

            SafeDeleteDirectory(root.FullName);
        }

        private static void SafeDeleteDirectory(string destinationDirectory)
        {
            const int tries = 10;
            for (var index = 1; index <= tries; index++)
            {
                try
                {
                    Directory.Delete(destinationDirectory, true);
                }
                catch (DirectoryNotFoundException)
                {
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(10);
                    continue;
                }
                return;
            }
        }

        /// <summary>
        /// Gets the content of files.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="contentFromFiles">The content from files.</param>
        private ExportFileResultModel GetContentOfFile(Tree tree, string fileName)
        {
            _repository = new Repository(_gitConnection.GitLocalFolder);
            foreach (var treeEntry in tree)
            {
                var gitObject = treeEntry.Target;
                var path = FileReaderExtensions.NormalizeFolderPath(treeEntry.Path);

                if (treeEntry.TargetType == TreeEntryTargetType.Tree)
                {
                    var output = GetContentOfFile((Tree)gitObject, fileName);
                    if (output != null)
                    {
                        return output;
                    }
                }
                else if (treeEntry.TargetType == TreeEntryTargetType.Blob)
                {
                    ExportFileResultModel result;
                    if (path == FileReaderExtensions.NormalizeFolderPath(fileName))
                    {
                        var blob = GetBlobFromFile(treeEntry);
                        result = new ExportFileResultModel(TextMimeType, Encoding.UTF8.GetBytes(blob), path);
                        return result;
                    }
                }
            }
            return null;
        }

        private static string GetBlobFromFile(TreeEntry treeEntry)
        {
            var blob = (Blob)treeEntry?.Target;
            var content = blob?.GetContentText();

            // strip BOM (U+FEFF) if present
            if (!string.IsNullOrWhiteSpace(content))
            {
                if (content[0] == '\uFEFF')
                {
                    content = content[1..];
                }
            }
            return content;
        }

        private static ObjectId GetCommitForTag(Tag tag)
        {
            EnsureArg.IsNotNull(tag);
            var peeledTarget = tag.PeeledTarget;
            if (peeledTarget is Commit)
            {
                // We're not interested by Tags pointing at Blobs or Trees
                var commitId = peeledTarget.Id;
                return commitId;
            }
            return null;
        }

        /// <summary>
        /// Helper function to check whether the ".git" 
        /// folder exists in a repository folder.
        /// </summary>
        /// <param name="pathToRep">path to a repository folder.</param>
        /// <returns></returns>
        private static bool IsGitSubDirPresent(string pathToRep)
        {
            return Directory.Exists(Path.Combine(pathToRep, GitFolder));
        }

        /// <summary>
        /// This method checks whether a folder 
        /// exists in a local directory
        /// and if there is ".git" sub folder
        /// within the same.
        /// </summary>
        /// <returns>
        /// true: if a folder and ".git" folder is present.
        /// false: if a folder and ".git" folder isn't present.
        /// </returns>
        private bool IsExistsLocalRepositoryDirectory()
        {
            return Directory.Exists(_gitConnection.GitLocalFolder) && IsGitSubDirPresent(_gitConnection.GitLocalFolder);
        }

        /// <summary>
        /// Determines whether [is local repository of remote].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is local repository of remote]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsLocalRepositorySameAsRemote()
        {
            _repository = new Repository(_gitConnection.GitLocalFolder);
            var remoteURL = _repository.Network.Remotes.First().Url;
            return remoteURL.Compares(_gitConnection.GitRemoteLocation);
        }

        private class MockSmartSubtransport : RpcSmartSubtransport
        {
            protected override SmartSubtransportStream Action(String url, GitSmartSubtransportAction action)
            {
                String endpointUrl, contentType = null;
                bool isPost = false;

                switch (action)
                {
                    case GitSmartSubtransportAction.UploadPackList:
                        endpointUrl = String.Concat(url, "/info/refs?service=git-upload-pack");
                        break;

                    case GitSmartSubtransportAction.UploadPack:
                        endpointUrl = String.Concat(url, "/git-upload-pack");
                        contentType = "application/x-git-upload-pack-request";
                        isPost = true;
                        break;

                    case GitSmartSubtransportAction.ReceivePackList:
                        endpointUrl = String.Concat(url, "/info/refs?service=git-receive-pack");
                        break;

                    case GitSmartSubtransportAction.ReceivePack:
                        endpointUrl = String.Concat(url, "/git-receive-pack");
                        contentType = "application/x-git-receive-pack-request";
                        isPost = true;
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                return new MockSmartSubtransportStream(this, endpointUrl, isPost, contentType);
            }

            private class MockSmartSubtransportStream : SmartSubtransportStream
            {
                private readonly static int MAX_REDIRECTS = 5;

                private readonly MemoryStream postBuffer = new MemoryStream();
                private Stream responseStream;

                public MockSmartSubtransportStream(MockSmartSubtransport parent, string endpointUrl, bool isPost, string contentType)
                    : base(parent)
                {
                    EndpointUrl = endpointUrl;
                    IsPost = isPost;
                    ContentType = contentType;
                }

                private string EndpointUrl
                {
                    get;
                    set;
                }

                private bool IsPost
                {
                    get;
                    set;
                }

                private string ContentType
                {
                    get;
                    set;
                }

                /// <summary>
                /// Writes the content of a given stream to the transport.
                /// </summary>
                /// <param name="dataStream">The stream with the data to write to the transport.</param>
                /// <param name="length">The number of bytes to read from <paramref name="dataStream" />.</param>
                /// <returns>
                /// The error code to propagate back to the native code that requested this operation. 0 is expected, and exceptions may be thrown.
                /// </returns>
                /// <exception cref="EndOfStreamException">Could not write buffer (short read)</exception>
                public override int Write(Stream dataStream, long length)
                {
                    byte[] buffer = new byte[4096];
                    long writeTotal = 0;

                    while (length > 0)
                    {
                        int readLen = dataStream.Read(buffer, 0, (int)Math.Min(buffer.Length, length));

                        if (readLen == 0)
                        {
                            break;
                        }

                        postBuffer.Write(buffer, 0, readLen);
                        length -= readLen;
                        writeTotal += readLen;
                    }

                    if (writeTotal < length)
                    {
                        throw new EndOfStreamException("Could not write buffer (short read)");
                    }

                    return 0;
                }

                private static HttpWebRequest CreateWebRequest(string endpointUrl, bool isPost, string contentType)
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(endpointUrl);
                    webRequest.UserAgent = "git/1.0 (libgit2 custom transport)";
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.AllowAutoRedirect = false;

                    if (isPost)
                    {
                        webRequest.Method = "POST";
                        webRequest.ContentType = contentType;
                    }

                    return webRequest;
                }

                private HttpWebResponse GetResponseWithRedirects()
                {
                    HttpWebRequest request = CreateWebRequest(EndpointUrl, IsPost, ContentType);
                    HttpWebResponse response = null;

                    for (int i = 0; i < MAX_REDIRECTS; i++)
                    {
                        if (IsPost && postBuffer.Length > 0)
                        {
                            postBuffer.Seek(0, SeekOrigin.Begin);

                            using (var requestStream = request.GetRequestStream())
                            {
                                postBuffer.WriteTo(requestStream);
                            }
                        }

                        response = (HttpWebResponse)request.GetResponse();

                        if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.Redirect)
                        {
                            request = CreateWebRequest(response.Headers["Location"], IsPost, ContentType);
                            continue;
                        }

                        break;
                    }

                    if (response == null)
                    {
                        throw new Exception("Too many redirects");
                    }

                    return response;
                }
                /// <summary>
                /// Reads the specified data stream.
                /// </summary>
                /// <param name="dataStream">The data stream.</param>
                /// <param name="length">The length.</param>
                /// <param name="readTotal">The read total.</param>
                /// <returns></returns>
                public override int Read(Stream dataStream, long length, out long readTotal)
                {
                    byte[] buffer = new byte[4096];
                    readTotal = 0;

                    if (responseStream == null)
                    {
                        HttpWebResponse response = GetResponseWithRedirects();
                        responseStream = response.GetResponseStream();
                    }

                    while (length > 0)
                    {
                        int readLen = responseStream.Read(buffer, 0, (int)Math.Min(buffer.Length, length));

                        if (readLen == 0)
                            break;

                        dataStream.Write(buffer, 0, readLen);
                        readTotal += readLen;
                        length -= readLen;
                    }

                    return 0;
                }

                protected override void Free()
                {
                    if (responseStream != null)
                    {
                        responseStream.Dispose();
                        responseStream = null;
                    }

                    base.Free();
                }
            }
        }

        #endregion
    }
}