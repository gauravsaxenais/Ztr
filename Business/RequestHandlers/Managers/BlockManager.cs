namespace Business.RequestHandlers.Managers
{
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

    public class BlockManager : Manager, IBlockManager
    {
        #region Private Variables
        private readonly IGitRepositoryManager _repoManager;
        private static readonly string fileArguments = "arguments";
        private static readonly string fileModules = "module";
        private static readonly string fileBlocks = "blocks";
        #endregion

        #region Constructors
        public BlockManager(ILogger<ModuleManager> logger, IGitRepositoryManager repoManager, IEnvironmentSettings environmentSettings) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(environmentSettings, nameof(environmentSettings));

            _repoManager = repoManager;

            var environmentOptions = environmentSettings.GetDeviceGitConnectionOptions();
            _repoManager.SetConnectionOptions(environmentOptions);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// toml file parser
        /// </summary>
        /// <param name="firmwareVersion"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public async Task<string> ParseTomlFilesAsync(string firmwareVersion, string deviceType, string parserType)
        {
            string finalJson = string.Empty;
            StringBuilder json = new StringBuilder();
            var gitConnectionOptions = _repoManager.GetConnectionOptions();

            string[] files = Directory.GetFiles(gitConnectionOptions.BlocksUrl);
            int fileIndex = 1;

            foreach (var currentFile in files)
            {
                string filename = Path.GetFullPath(currentFile);
                TextReader readFile = new StreamReader(filename);
                string content = readFile.ReadToEnd();
                var fileContent = Toml.ReadString(content);

                string strData = string.Empty;

                var dictionary = fileContent.ToDictionary(t => t.Key, t => (object)t.Value.ToString());
                List<object> flattenList;

                if (parserType.Contains(fileModules))
                {
                    flattenList = dictionary.Where(x => x.Key == fileModules).Select(x => x.Value).ToList();
                    ModuleParserToJson(currentFile, ref json, ref readFile, ref strData, flattenList);
                }
                else if (parserType.Contains(fileBlocks))
                {
                    flattenList = dictionary.Where(x => x.Key == fileArguments).Select(x => x.Value).ToList();
                    BlockParserToJson(currentFile, ref json, ref readFile, ref strData, flattenList, fileIndex);
                }
                fileIndex++;
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
        /// <summary>
        /// Block parser
        /// </summary>
        /// <param name="currentFile"></param>
        /// <param name="json"></param>
        /// <param name="readFile"></param>
        /// <param name="strData"></param>
        /// <param name="flattenList"></param>
        /// <param name="fileIndex"></param>
        private static void BlockParserToJson(string currentFile, ref StringBuilder json, ref TextReader readFile,
            ref string strData, List<object> flattenList, int fileIndex)
        {
            if (flattenList != null && flattenList.Any())
            {
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

                foreach (var item in tempData)
                {
                    strData =
                        item
                        .Replace("=", "\":")
                        .Replace(",", ",\"") + "},";

                    json.Append(strData);
                }

                json = json
                    .Replace("]}},", "}")
                    .Replace("[,", "[");

                readFile.Close();
                readFile = null;
            }
        }

        /// <summary>
        /// Module parser
        /// </summary>
        /// <param name="currentFile"></param>
        /// <param name="json"></param>
        /// <param name="readFile"></param>
        /// <param name="strData"></param>
        /// <param name="flattenList"></param>
        /// <param name="newDir"></param>
        private static void ModuleParserToJson(string currentFile, ref StringBuilder json, ref TextReader readFile,
            ref string strData, List<object> flattenList)
        {
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
                    strData = "{" + item
                        .Replace("{", "{\"")
                        .Replace("=", "\":")
                        .Replace(",", ",\"")
                        .Replace("\"\",\"", "\",\"") + "},";

                    json.AppendLine(strData);
                }

                json = json
                    .Replace("]},", "]");

                readFile.Close();
                readFile = null;
            }
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
