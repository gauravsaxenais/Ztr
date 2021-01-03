namespace Business.RequestHandlers.Managers
{
    using Interfaces;
    using EnsureThat;
    using LibGit2Sharp;
    using LibGit2Sharp.Handlers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business.File;
    using ZTR.Framework.Business.File.FileReaders;
    using ZTR.Framework.Business.Models;
    using Blob = LibGit2Sharp.Blob;

    /// <summary>
    /// Git repo manager is responsible for cloning a git repo,
    /// gets all the tags.
    /// </summary>
    /// <seealso cref="IGitRepositoryManager" />
    public class GitRepositoryManager : IGitRepositoryManager
    {
        #region Fields
        private UsernamePasswordCredentials _credentials;
        private DefaultCredentials _defaultCredentials;
        private GitConnectionOptions _gitConnection;
        private CloneOptions _cloneOptions;
        private readonly string GitFolder = ".git";
        private readonly string TextMimeType = "text/plain";
        #endregion

        #region Constructors        

        /// <summary>
        /// Initializes a new instance of the <see cref="GitRepositoryManager"/> class.
        /// </summary>
        public GitRepositoryManager()
        {
        }
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
            EnsureArg.IsNotEmptyOrWhiteSpace(gitConnection.GitRepositoryUrl);

            _gitConnection = gitConnection;

            _credentials = new UsernamePasswordCredentials
            {
                Username = gitConnection.UserName,
                Password = gitConnection.Password
            };

            _defaultCredentials = new DefaultCredentials();

            var credentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => _defaultCredentials);
            
            _cloneOptions = new CloneOptions() { CredentialsProvider = credentialsProvider };
            _cloneOptions.CertificateCheck += delegate (Certificate certificate, bool valid, string host)
            {
                return true;
            };
        }

        /// <summary>
        /// Gets the connection options.
        /// </summary>
        /// <returns></returns>
        public GitConnectionOptions GetConnectionOptions()
        {
            return _gitConnection;
        }

        /// <summary>
        /// Clones the repository asynchronous.
        /// </summary>
        /// <exception cref="Exception">
        /// Unauthorized: Incorrect username/password
        /// or
        /// Forbidden: Possibly Incorrect username/password
        /// or
        /// Not found: The repository was not found
        /// </exception>
        public async Task CloneRepositoryAsync()
        {
            try
            {
                if (IsExistsContentRepositoryDirectory())
                {
                    DeleteReadOnlyDirectory(_gitConnection.GitLocalFolder);
                }

                Directory.CreateDirectory(_gitConnection.GitLocalFolder);
                
                await Task.Run(() =>
                {
                    Repository.Clone(_gitConnection.GitRepositoryUrl, _gitConnection.GitLocalFolder, _cloneOptions);
                });
            }
            catch (LibGit2SharpException ex)
            {
                var message = ex.Message;
                if (message.Contains("401"))
                {
                    throw new CustomArgumentException("Unauthorized: Incorrect username/password");
                }
                if (message.Contains("403"))
                {
                    throw new CustomArgumentException("Forbidden: Possibly Incorrect username/password");
                }
                if (message.Contains("404"))
                {
                    throw new CustomArgumentException("Not found: The repository was not found");
                }

                throw;
            }
        }

        /// <summary>
        /// Loads the tag names asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllTagNamesAsync()
        {
            var tags = new List<(string, DateTimeOffset)>();
            tags = await GetAllTagsAsync().ConfigureAwait(false);

            return tags.Select(x => x.Item1).ToList();
        }

        /// <summary>
        /// Gets the tags earlier than this tag asynchronous.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
        public async Task<List<string>> GetTagsEarlierThanThisTagAsync(string tagName)
        {
            var tags = new List<(string, DateTimeOffset)>();
            tags = await GetAllTagsAsync().ConfigureAwait(false);

            var tag = tags.FirstOrDefault(x => x.Item1 == tagName);

            var tagNames = tags.Where(x => x.Item2 < tag.Item2).Select(x => x.Item1).ToList();

            return tagNames;
        }

        /// <summary>
        /// This method returns a filedata as string from a particular tag.
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

            var tagsPerPeeledCommitId = new Dictionary<ObjectId, List<Tag>>();
            var listOfContentFiles = new List<ExportFileResultModel>();

            if (!IsExistsContentRepositoryDirectory())
            {
                await CloneRepositoryAsync().ConfigureAwait(false);
            }

            using (var repo = new Repository(_gitConnection.GitLocalFolder))
            {
                tagsPerPeeledCommitId = TagsPerPeeledCommitId(repo.Tags);

                // Let's enumerate all the reachable commits (similarly to `git log --all`)
                foreach (Commit commit in repo.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = repo.Refs }))
                {
                    foreach (var tags in AssignedTags(commit, tagsPerPeeledCommitId))
                    {
                        if (tags.FriendlyName == tag)
                        {
                            GetContentOfFiles(repo, commit.Tree, listOfContentFiles);

                            // case insensitive search.
                            listOfContentFiles = listOfContentFiles.Where(p => p.FileName?.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                        }
                    }
                }
            }

            return listOfContentFiles;
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Checks a GIT tree to see if a file exists
        /// </summary>
        /// <param name="tree">The GIT tree</param>
        /// <param name="fileName">The file name</param>
        /// <returns>true if file exists</returns>
        private bool TreeContainsFile(Tree tree, string fileName)
        {
            if (tree.Any(x => x.Path.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            foreach (TreeEntry branch in tree.Where(x => x.TargetType == TreeEntryTargetType.Tree))
            {
                if (this.TreeContainsFile((Tree)branch.Target, fileName))
                {
                    return true;
                }
            }

            return false;
        }

        private void GetContentOfFiles(IRepository repo, Tree tree, List<ExportFileResultModel> contentfromFiles)
        {
            foreach (var treeEntry in tree)
            {
                var gitObject = treeEntry.Target;
                var path = treeEntry.Path;

                if (treeEntry.TargetType == TreeEntryTargetType.Tree)
                {
                    GetContentOfFiles(repo, (Tree)gitObject, contentfromFiles);
                }

                if (treeEntry.TargetType == TreeEntryTargetType.Blob)
                {
                    var blob = GetBlobFromFile(treeEntry);
                    contentfromFiles.Add(new ExportFileResultModel(TextMimeType, System.Text.Encoding.UTF8.GetBytes(blob), path));
                }
            }
        }

        private async Task<List<(string, DateTimeOffset)>> GetAllTagsAsync()
        {
            var tags = new List<Tag>();
            var tagNames = new List<(string, DateTimeOffset)>();

            if (!IsExistsContentRepositoryDirectory())
            {
                await CloneRepositoryAsync().ConfigureAwait(false);
            }

            using (var repo = new Repository(_gitConnection.GitLocalFolder))
            {
                // Add new tags.
                foreach (var tag in repo.Tags)
                {
                    GitObject peeledTarget = tag.PeeledTarget;

                    if (peeledTarget is Commit temp)
                    {
                        var date = temp.Author.When;

                        // We're not interested by Tags pointing at Blobs or Trees
                        // only interested in tags for a commit.
                        tags.Add(tag);
                    }
                }

                tagNames = SortTags(tags: tags, order: t => ((Commit)t.PeeledTarget).Author.When,
                                        selector: t => (t.FriendlyName, ((Commit)t.PeeledTarget).Author.When));
            }

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

        private static IEnumerable<Tag> AssignedTags(Commit commit, Dictionary<ObjectId, List<Tag>> tags)
        {
            if (!tags.ContainsKey(commit.Id))
            {
                return Enumerable.Empty<Tag>();
            }

            return tags[commit.Id];
        }

        private Dictionary<ObjectId, List<Tag>> TagsPerPeeledCommitId(IEnumerable<Tag> listOfTags)
        {
            var tagsPerPeeledCommitId = new Dictionary<ObjectId, List<Tag>>();

            if (listOfTags != null && listOfTags.Any())
            {
                foreach (Tag tag in listOfTags)
                {
                    GitObject peeledTarget = tag.PeeledTarget;

                    if (!(peeledTarget is Commit))
                    {
                        // We're not interested by Tags pointing at Blobs or Trees
                        continue;
                    }

                    ObjectId commitId = peeledTarget.Id;

                    if (!tagsPerPeeledCommitId.ContainsKey(commitId))
                    {
                        // A Commit may be pointed at by more than one Tag
                        tagsPerPeeledCommitId.Add(commitId, new List<Tag>());
                    }

                    tagsPerPeeledCommitId[commitId].Add(tag);
                }
            }

            return tagsPerPeeledCommitId;
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
            if (Directory.Exists(_gitConnection.GitLocalFolder))
            {
                if (CheckForGitSubDir(_gitConnection.GitLocalFolder))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Helper function to check whether the ".git" 
        /// folder exists in a repository folder.
        /// </summary>
        /// <param name="pathToRep">path to a repository folder.</param>
        /// <returns></returns>
        private bool CheckForGitSubDir(string pathToRep)
        {
            if (Directory.Exists(Path.Combine(pathToRep, GitFolder)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
        /// </summary>
        /// <param name="directory">The name of the directory to remove.</param>
        private void DeleteReadOnlyDirectory(string directory)
        {
            try
            {
                foreach (var subdirectory in Directory.EnumerateDirectories(directory))
                {
                    DeleteReadOnlyDirectory(subdirectory);
                }
                foreach (var fileName in Directory.EnumerateFiles(directory))
                {
                    var fileInfo = new FileInfo(fileName)
                    {
                        Attributes = FileAttributes.Normal
                    };

                    if (FileReaderExtensions.IsFileClosed(fileName))
                    {
                        fileInfo.Delete();
                    }
                }

                Directory.Delete(directory);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}