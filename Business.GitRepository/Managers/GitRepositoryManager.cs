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
    using System.Threading.Tasks;
    using ZTR.Framework.Business.File;
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
        public async Task CloneRepositoryAsync()
        {
            lock (_syncRoot)
            {

                // clone only when there is a change.
                if (IsExistsContentRepositoryDirectory())
                {
                    GetLatestFromRepository();
                }
                else
                {
                    Directory.CreateDirectory(_gitConnection.GitLocalFolder);

                    var cloneOptions = new CloneOptions();
                    cloneOptions.CertificateCheck += (certificate, valid, host) => true;
                    try
                    {
                        cloneOptions.CredentialsProvider += (_url, _user, _cred) => new DefaultCredentials();
                        Repository.Clone(_gitConnection.GitRemoteLocation, _gitConnection.GitLocalFolder,
                        cloneOptions);
                    }
                    catch (Exception)
                    {
                        cloneOptions.CredentialsProvider = (_url, _user, _cred) => _userNamePasswordCredentials;
                        Repository.Clone(_gitConnection.GitRemoteLocation, _gitConnection.GitLocalFolder,
                        cloneOptions);
                    }

                    _repository = new Repository(_gitConnection.GitLocalFolder);
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
        public bool IsExistsContentRepositoryDirectory()
        {
            return Directory.Exists(_gitConnection.GitLocalFolder) && IsGitSubDirPresent(_gitConnection.GitLocalFolder);
        }

        #endregion

        #region Private methods

        private void GetLatestFromRepository()
        {
            _repository = new Repository(_gitConnection.GitLocalFolder);

            var network = _repository.Network.Remotes.First();
            var refSpecs = new List<string>() { network.FetchRefSpecs.First().Specification };

            var fetchOptions = new FetchOptions { TagFetchMode = TagFetchMode.All };

            try
            {
                fetchOptions.CredentialsProvider += (_url, _user, _cred) => new DefaultCredentials();
                _repository.Network.Fetch(network.Name, refSpecs, fetchOptions);
            }
            catch (Exception)
            {
                fetchOptions.CredentialsProvider = (_url, _user, _cred) => _userNamePasswordCredentials;
                _repository.Network.Fetch(network.Name, refSpecs, fetchOptions);
            }
        }

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

            return await Task.FromResult(tagNames);
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
        /// Helper function to check whether the ".git" 
        /// folder exists in a repository folder.
        /// </summary>
        /// <param name="pathToRep">path to a repository folder.</param>
        /// <returns></returns>
        private bool IsGitSubDirPresent(string pathToRep)
        {
            return Directory.Exists(Path.Combine(pathToRep, GitFolder));
        }

        #endregion
    }
}