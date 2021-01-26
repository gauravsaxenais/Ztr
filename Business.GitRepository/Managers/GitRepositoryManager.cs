namespace Business.GitRepository.Managers
{
    using EnsureThat;
    using Interfaces;
    using LibGit2Sharp;
    using LibGit2Sharp.Handlers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File;
    using ZTR.Framework.Business.Models;
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
        private CloneOptions _cloneOptions;
        private const string GitFolder = ".git";
        private const string TextMimeType = "text/plain";
        private Repository _repository;
        private UsernamePasswordCredentials _credentials;
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

            _credentials = new UsernamePasswordCredentials()
            {
                Username = gitConnection.UserName,
                Password = gitConnection.Password
            };

            var credentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => _credentials);

            _cloneOptions = new CloneOptions() { CredentialsProvider = credentialsProvider };
            _cloneOptions.CertificateCheck += (certificate, valid, host) => true;
        }

        /// <summary>
        /// Clones the repository asynchronous.
        /// </summary>
        /// <exception cref="Exception">
        /// </exception>/
        public async Task CloneRepositoryAsync()
        {
            lock (_syncRoot)
            {
                try
                {
                    if (IsExistsContentRepositoryDirectory())
                    {
                        DeleteReadOnlyDirectory(_gitConnection.GitLocalFolder);
                    }

                    Directory.CreateDirectory(_gitConnection.GitLocalFolder);

                    Repository.Clone(_gitConnection.GitRemoteLocation, _gitConnection.GitLocalFolder,
                        _cloneOptions);

                    _repository = new Repository(_gitConnection.GitLocalFolder);
                }
                catch (LibGit2SharpException ex)
                {
                    throw new CustomArgumentException("System is unable to retrieve git repository information. Please try after sometime.", ex);
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Loads the tag names asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllTagNamesAsync()
        {
            try
            {
                var tags = await GetAllTagsAsync().ConfigureAwait(false);
                var tagNames = tags.Select(x => x.Item1).ToList();

                return tagNames;
            }
            catch (LibGit2SharpException ex)
            {
                throw new CustomArgumentException("Unable to get all tag names.", ex);
            }
        }

        /// <summary>
        /// Gets the tags earlier than this tag asynchronous.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
        public async Task<List<string>> GetTagsEarlierThanThisTagAsync(string tagName)
        {
            try
            {
                var tags = await GetAllTagsAsync().ConfigureAwait(false);

                var tag = tags.FirstOrDefault(x => x.Item1 == tagName);

                var tagNames = tags.Where(x => x.Item2 < tag.Item2).Select(x => x.Item1).ToList();

                return tagNames;
            }
            catch (LibGit2SharpException ex)
            {
                throw new CustomArgumentException($"Unable to get tags earlier than {tagName} from git repo.", ex);
            }
        }

        /// <summary>
        /// This method returns a file data as string from a particular tag.
        /// If a tag is "1.0.7", then the method returns data from all the files
        /// for a particular tag.
        /// </summary>
        /// <param name="tag">Tag like 1.0.7 etc. in github.</param>
        /// <param name="fileName">file name to search</param>
        /// <returns></returns>
        public async Task<IEnumerable<ExportFileResultModel>> GetFileDataFromTagAsync(string tag, string fileName)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(tag);
            EnsureArg.IsNotEmptyOrWhiteSpace(fileName);

            try
            {
                var listOfContentFiles = new List<ExportFileResultModel>();

                if (!IsExistsContentRepositoryDirectory())
                {
                    await CloneRepositoryAsync().ConfigureAwait(false);
                }

                _repository = new Repository(_gitConnection.GitLocalFolder);

                var repoTag = _repository.Tags.FirstOrDefault(item => item.FriendlyName == tag);

                var commitForTag = GetAllCommitsForTag(repoTag);

                // Let's enumerate all the reachable commits (similarly to `git log --all`)
                foreach (var commit in _repository.Commits.QueryBy(new CommitFilter
                { IncludeReachableFrom = _repository.Refs }))
                {
                    if (commit.Id == commitForTag)
                    {
                        GetContentOfFiles(_repository, commit.Tree, listOfContentFiles);

                        // case insensitive search.
                        listOfContentFiles = listOfContentFiles.Where(p =>
                            p.FileName?.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    }
                }

                return listOfContentFiles;
            }

            catch (LibGit2SharpException exception)
            {
                throw new CustomArgumentException($"Unable to get file for a particular {tag} from git repo.", exception);
            }
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

        private void GetContentOfFiles(IRepository repo, Tree tree, ICollection<ExportFileResultModel> contentFromFiles)
        {
            foreach (var treeEntry in tree)
            {
                var gitObject = treeEntry.Target;
                var path = treeEntry.Path;

                if (treeEntry.TargetType == TreeEntryTargetType.Tree)
                {
                    GetContentOfFiles(repo, (Tree)gitObject, contentFromFiles);
                }

                else if (treeEntry.TargetType == TreeEntryTargetType.Blob)
                {
                    var blob = GetBlobFromFile(treeEntry);
                    contentFromFiles.Add(new ExportFileResultModel(TextMimeType, System.Text.Encoding.UTF8.GetBytes(blob), path));
                }
            }
        }

        private async Task<List<(string, DateTimeOffset)>> GetAllTagsAsync()
        {
            var tags = new List<Tag>();

            if (!IsExistsContentRepositoryDirectory())
            {
                await CloneRepositoryAsync().ConfigureAwait(false);
            }

            _repository = new Repository(_gitConnection.GitLocalFolder);

            // Add new tags.
            foreach (var tag in _repository.Tags)
            {
                var peeledTarget = tag.PeeledTarget;

                if (peeledTarget is Commit)
                {
                    // We're not interested by Tags pointing at Blobs or Trees
                    // only interested in tags for a commit.
                    tags.Add(tag);
                }
            }

            var tagNames = tags.OrderByDescending(t => ((Commit)t.PeeledTarget).Author.When)
                                                  .ThenByDescending(t => t.FriendlyName)
                                                  .Select(t => (t.FriendlyName, ((Commit)t.PeeledTarget).Author.When))
                                                  .ToList();

            return tagNames;
        }

        /// <summary>
        /// Sorts the tags.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tags">The tags.</param>
        /// <param name="order">The order.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="where">The where.</param>
        /// <returns></returns>
        private static List<T> SortTags<T>(IEnumerable<Tag> tags, Func<Tag, object> order, Func<Tag, T> selector, Func<Tag, bool> where = null)
        {
            return where == null ? tags.OrderByDescending(order).Select(selector).ToList() :
                tags.Where(where).OrderByDescending(order).Select(selector).ToList();
        }

        private string GetBlobFromFile(TreeEntry treeEntry)
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

        private ObjectId GetAllCommitsForTag(Tag tag)
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
        /// This method checks whether a folder 
        /// exists in a local directory
        /// and if there is ".git" sub folder
        /// within the same.
        /// </summary>
        /// <returns>
        /// true: if a folder and ".git" folder is present.
        /// false: if a folder and ".git" folder isn't present.
        /// </returns>
        private bool IsExistsContentRepositoryDirectory()
        {
            return Directory.Exists(_gitConnection.GitLocalFolder) && IsGitSubDirPresent(_gitConnection.GitLocalFolder);
        }

        /// <summary>
        /// Helper function to check whether the ".git" 
        /// folder exists in a repository folder.
        /// </summary>
        /// <param name="pathToRep">path to a repository folder.</param>
        /// <returns></returns>
        private bool IsGitSubDirPresent(string pathToRep)
        {
            return Directory.Exists(Path.Combine(pathToRep, GitFolder));
        }

        /// <summary>
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
        /// </summary>
        /// <param name="directory">The name of the directory to remove.</param>
        private void DeleteReadOnlyDirectory(string directory)
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

        private void SafeDeleteDirectory(string destinationDirectory)
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

            throw new CustomArgumentException($"There is an issue accessing the git repository.");
        }

        #endregion
    }
}