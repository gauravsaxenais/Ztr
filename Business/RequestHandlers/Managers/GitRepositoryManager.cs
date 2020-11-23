namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using LibGit2Sharp;
    using LibGit2Sharp.Handlers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business.File;
    using Blob = LibGit2Sharp.Blob;

    public class GitRepositoryManager : IGitRepositoryManager
    {
        #region Fields
        private UsernamePasswordCredentials _credentials;
        private DeviceGitConnectionOptions _gitConnection;
        private readonly string GitFolder = ".git";
        private readonly string TextMimeType = "text/plain";
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GitRepositoryManager" /> class.
        /// </summary>
        ///
        /// <param name="gitConnection">The Git connection options from appsettings.</param>
        public GitRepositoryManager()
        {
        }
        #endregion

        #region Public methods
        public void SetConnectionOptions(DeviceGitConnectionOptions gitConnection)
        {
            EnsureArg.IsNotNull(gitConnection);
            EnsureArg.IsNotNull(gitConnection.TomlConfiguration);
            EnsureArg.IsNotEmptyOrWhiteSpace(gitConnection.UserName);
            EnsureArg.IsNotEmptyOrWhiteSpace(gitConnection.Password);
            EnsureArg.IsNotEmptyOrWhiteSpace(gitConnection.GitLocalFolder);
            EnsureArg.IsNotEmptyOrWhiteSpace(gitConnection.GitRepositoryUrl);
            EnsureArg.IsNotEmptyOrWhiteSpace(gitConnection.TomlConfiguration.DeviceFolder);

            _gitConnection = gitConnection;

            _credentials = new UsernamePasswordCredentials
            {
                Username = gitConnection.UserName,
                Password = gitConnection.Password
            };
        }

        public DeviceGitConnectionOptions GetConnectionOptions()
        {
            return _gitConnection;
        }

        public async Task CloneRepositoryAsync()
        {
            if (!IsExistsContentRepositoryDirectory())
            {
                Directory.CreateDirectory(_gitConnection.GitLocalFolder);
                await Task.Run(() =>
                {
                    // The following modification allowed me to fetch from repositories over LAN
                    var cloneOptions = new CloneOptions
                    {
                        CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => new DefaultCredentials())
                    };

                    cloneOptions.CertificateCheck += delegate (Certificate certificate, bool valid, string host)
                    {
                        return true;
                    };

                    Repository.Clone(_gitConnection.GitRepositoryUrl, _gitConnection.GitLocalFolder, cloneOptions);
                });
            }
            else
            {
                using var repo = new Repository(_gitConnection.GitLocalFolder);
                var network = repo.Network.Remotes.First();
                var refSpecs = new List<string>() { network.FetchRefSpecs.First().Specification };

                var fetchOptions = new FetchOptions { TagFetchMode = TagFetchMode.All };

                fetchOptions.CredentialsProvider += (url, fromUrl, types) => _credentials;
                repo.Network.Fetch(network.Name, refSpecs, fetchOptions);
            }
        }

        /// <summary>
        /// Loads all the tags.
        /// </summary>
        public async Task<string[]> LoadTagNamesAsync()
        {
            string[] tagNames;
            var tags = new List<Tag>();

            await CloneRepositoryAsync();

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

                tagNames = SortedTags(tags: tags, order: t => ((Commit)t.PeeledTarget).Author.When,
                                        selector: t => t.FriendlyName);
            }

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

            await CloneRepositoryAsync();

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
        /// <param name="filename">The file name</param>
        /// <returns>true if file exists</returns>
        private bool TreeContainsFile(Tree tree, string fileName)
        {
            if (tree.Any(x => x.Path.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }
            else
            {
                foreach (TreeEntry branch in tree.Where(x => x.TargetType == TreeEntryTargetType.Tree))
                {
                    if (this.TreeContainsFile((Tree)branch.Target, fileName))
                    {
                        return true;
                    }
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

        private static T[] SortedTags<T>(IEnumerable<Tag> tags, Func<Tag, object> order, Func<Tag, T> selector)
        {
            return tags.OrderByDescending(order).Select(selector).ToArray();
        }

        private string GetBlobFromFile(TreeEntry treeEntry)
        {
            var blob = (Blob)treeEntry?.Target;
            string content = blob?.GetContentText();

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
        #endregion
    }
}
