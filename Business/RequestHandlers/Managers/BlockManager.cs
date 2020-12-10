namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Nett;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    /// <summary>
    /// Parses a toml file and returns arguments for a block
    /// among others.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IBlockManager" />
    public class BlockManager : Manager, IBlockManager
    {
        #region Private Variables
        private readonly IGitRepositoryManager _repoManager;
        private readonly BlockGitConnectionOptions _blockGitConnectionOptions;

        private static readonly string fileArguments = "arguments";
        private static readonly string fileModules = "module";
        private static readonly string fileBlocks = "blocks";
        #endregion

        #region Constructors        

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="blockGitConnectionOptions">The block git connection options.</param>
        public BlockManager(ILogger<ModuleManager> logger, IGitRepositoryManager gitRepoManager, BlockGitConnectionOptions blockGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(blockGitConnectionOptions, nameof(blockGitConnectionOptions));

            _repoManager = gitRepoManager;
            _blockGitConnectionOptions = blockGitConnectionOptions;

            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _blockGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _blockGitConnectionOptions.GitLocalFolder);
            _blockGitConnectionOptions.BlockConfig = Path.Combine(currentDirectory, _blockGitConnectionOptions.GitLocalFolder, _blockGitConnectionOptions.BlockConfig);
            _repoManager.SetConnectionOptions(_blockGitConnectionOptions);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Parses the toml files asynchronous.
        /// </summary>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="parserType">Type of the parser.</param>
        /// <returns></returns>
        public async Task<string> ParseTomlFilesAsync(string firmwareVersion, string deviceType, string parserType)
        {
            string finalJson = string.Empty;
            var json = new StringBuilder();
            var gitConnectionOptions = (BlockGitConnectionOptions)_repoManager.GetConnectionOptions();

            await _repoManager.CloneRepositoryAsync();

            int fileIndex = 1;

            var directory = new DirectoryInfo(gitConnectionOptions.BlockConfig);

            foreach (var currentFile in directory.EnumerateFiles())
            {
                string content = File.ReadAllText(currentFile.FullName);
                var fileContent = Toml.ReadString(content);

                string strData = string.Empty;

                var dictionary = fileContent.ToDictionary(t => t.Key, t => (object)t.Value.ToString());
                List<object> flattenList;

                if (parserType.Contains(fileModules))
                {
                    flattenList = dictionary.Where(x => x.Key == fileModules).Select(x => x.Value).ToList();
                    json.Append(ModuleParserToJson(currentFile.Name, flattenList));
                }
                else if (parserType.Contains(fileBlocks))
                {
                    flattenList = dictionary.Where(x => x.Key == fileArguments).Select(x => x.Value).ToList();
                    if (flattenList != null && flattenList.Any())
                    {
                        json.Append(BlockParserToJson(currentFile.Name, flattenList, fileIndex));
                        fileIndex++;
                    }
                }
            }

            if (parserType.Contains(fileModules))
            {
                finalJson = "{\"" + parserType + "\":[" + json.Append("]}").ToString().Replace(",\"},", "}");
            }
            else
            {
                finalJson = json.Insert(0, "{\"" + parserType + "\":[").Append("]}").ToString().Trim().Replace("}]},]}", "}]}]}");
            }

            if (!IsValidJson(finalJson))
            {
                finalJson = string.Empty;
            }

            return finalJson;
        }
        #endregion

        #region Private Methods

        /// <summary>Blocks the parser to json.</summary>
        /// <param name="currentFile">The current file.</param>
        /// <param name="flattenList">The flatten list.</param>
        /// <param name="fileIndex">Index of the file.</param>
        /// <returns>
        ///   json as stringbuilder.
        /// </returns>
        private StringBuilder BlockParserToJson(string currentFile, List<object> flattenList, int fileIndex)
        {
            var json = new StringBuilder();

            json.Insert(json.Length, "{\"id\":"
                + fileIndex
                + ",\"type\":\""
                + Path.GetFileNameWithoutExtension(currentFile)
                + "\",\"tag\":" + "\"\","
                + "\"args\":[");

            string[] tempData = Convert.ToString(flattenList[0])
                .Replace("_ =", "")
                .Replace("\r\n", "")
                .Replace(" ", "")
                .Replace("[", "")
                .Replace("{", "{\"")
                .Split("},");

            if (json.Length > 0)
            {
                json.Append(',');
            }

            int argIndex = 1;
            foreach (var item in tempData)
            {
                string strData =
                                item
                                .Replace("=", "\":")
                                .Replace(",", ",\"") + "},";
                json.Append(strData.Replace("{\"", "{\"id\":" + argIndex + ",\""));

                argIndex++;
            }

            json = json
                .Replace("]}},", "}")
                .Replace("[,", "[");

            return json;
        }

        /// <summary>Modules the parser to json.</summary>
        /// <param name="currentFile">The current file.</param>
        /// <param name="flattenList">The flatten list.</param>
        /// <returns>
        ///   json as stringbuilder.
        /// </returns>
        private StringBuilder ModuleParserToJson(string currentFile, List<object> flattenList)
        {
            var json = new StringBuilder();
            
            if (flattenList != null && flattenList.Any())
            {
                string[] tempData =
                    Convert.ToString(
                        flattenList[0])
                        .Replace("_ =", "")
                        .Replace("\r\n", "")
                        .Replace(" ", "")
                        .Replace("[", "\"" + Path.GetFileNameWithoutExtension(currentFile) + "\":[")
                        .Replace("[\"device\":[_]]", "[{")
                        .Replace("\"device\":[{", ",\"")
                        .Replace("\"", "\"\",")
                        .Replace(",\"\",", "\"")
                        .Replace("=\"", "\":")
                        .Replace(":\",", ":\"")
                        .Replace("\"\",\"", "\"},\"")
                        .Split("},");

                if (json.Length > 0)
                {
                    json.Append(',');
                }
                foreach (var item in tempData)
                {
                    string strData = "{" + item
                                    .Replace("{", "{\"")
                                    .Replace("=", "\":")
                                    .Replace(",", ",\"")
                                    .Replace("\"\",\"", "\",\"") + "},";
                    
                    json.AppendLine(strData);
                }

                json = json
                    .Replace("]},", "]");
            }

            return json;
        }

        private static bool IsValidJson(string strInput)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(strInput, nameof(strInput));

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    //Exception in parsing json
                    return false;
                }
                catch (Exception) //some other exception
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
