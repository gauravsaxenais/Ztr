namespace Business.GitRepository.ZTR.M7Config.Business
{
    using EnsureThat;
    using global::ZTR.Framework.Business;
    using global::ZTR.Framework.Business.File.FileReaders;
    using global::ZTR.M7Config.Business.Common.Configuration;
    using global::ZTR.M7Config.Business.Common.Models;
    using global::ZTR.M7Config.Business.GitRepository.Interfaces;
    using Microsoft.Extensions.Logging;
    using Nett;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper for GitRepoManager.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleServiceManager" />
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleServiceManager" />
    public class ModuleServiceManager : IModuleServiceManager
    {
        private readonly string protoFileName = "module.proto";
        private readonly ILogger<ModuleServiceManager> _logger;
        private readonly IGitRepositoryManager _repoManager;
        private readonly ModuleGitConnectionOptions _moduleGitConnectionOptions;
        private const string Prefix = nameof(ModuleServiceManager);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="firmwareVersionServiceManager">The firmware version service manager.</param>
        public ModuleServiceManager(ILogger<ModuleServiceManager> logger, ModuleGitConnectionOptions moduleGitConnectionOptions, IGitRepositoryManager gitRepoManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(moduleGitConnectionOptions, nameof(moduleGitConnectionOptions));
            EnsureArg.IsNotNull(gitRepoManager, nameof(gitRepoManager));

            _logger = logger;
            _moduleGitConnectionOptions = moduleGitConnectionOptions;
            _repoManager = gitRepoManager;

            SetConnection();
        }

        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="listOfModules">The list of modules.</param>
        public async Task UpdateMetaTomlAsync(List<ModuleReadModel> listOfModules)
        {
            string metaTomlFilePath = _moduleGitConnectionOptions.MetaToml;
            string moduleFilePath = _moduleGitConnectionOptions.GitLocalFolder;

            foreach (var module in listOfModules)
            {
                module.IconUrl = GetModuleIconUrl(module, moduleFilePath, metaTomlFilePath);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Clones the git hub repo asynchronous.
        /// </summary>
        public async Task CloneGitRepoAsync()
        {
            _logger.LogInformation($"Cloning github repository for modules.");
            await _repoManager.InitRepositoryAsync().ConfigureAwait(false);
            _logger.LogInformation($"Github repository cloning is successful for modules.");
        }

        /// <summary>
        /// Gets the proto files.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        public string GetProtoFiles(ModuleReadModel module)
        {
            EnsureArg.IsNotNull(module);

            string moduleFilePath = _moduleGitConnectionOptions.GitLocalFolder;
            var moduleFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFilePath, module.Name);

            if (string.IsNullOrWhiteSpace(moduleFolder))
            {
                return string.Empty;
            }

            var uuidFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFolder, module.UUID);

            if (!string.IsNullOrWhiteSpace(uuidFolder))
            {
                foreach (var file in Directory.EnumerateFiles(uuidFolder, protoFileName))
                {
                    return file;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the connection.
        /// </summary>
        /// <param name="connectionOptions">The connection options.</param>
        public void SetConnection()
        {
            _logger.LogInformation("Setting git repository connection");
            var appPath = GlobalMethods.GetCurrentAppPath();
            _moduleGitConnectionOptions.GitLocalFolder = Path.Combine(appPath, _moduleGitConnectionOptions.GitLocalFolder);
            _repoManager.SetConnectionOptions(_moduleGitConnectionOptions);
        }

        /// <summary>
        /// Gets the module icon URL.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="moduleFilePath">The module file path.</param>
        /// <param name="metaTomlFilePath">The meta toml file path.</param>
        /// <returns></returns>
        private static string GetModuleIconUrl(ModuleReadModel module, string moduleFilePath, string metaTomlFilePath)
        {
            EnsureArg.IsNotNull(module);
            var iconUrl = string.Empty;

            var moduleFolder = FileReaderExtensions.GetSubDirectoryPath(moduleFilePath, module.Name);

            if (string.IsNullOrWhiteSpace(moduleFolder))
            {
                return string.Empty;
            }

            var metaTomlFile = Path.Combine(moduleFolder, metaTomlFilePath);

            try
            {
                if (File.Exists(metaTomlFile))
                {
                    var tml = Toml.ReadFile(metaTomlFile, TomlFileReader.LoadLowerCaseTomlSettings());

                    var dict = tml.ToDictionary();
                    var moduleValues = dict["module"];

                    if (moduleValues is Dictionary<string, object>)
                    {
                        var moduleFromToml = (Dictionary<string, object>)dict["module"];
                        if (moduleFromToml != null && (string)moduleFromToml["name"] == module.Name)
                        {
                            iconUrl = moduleFromToml["iconUrl"].ToString();

                            if (!iconUrl.IsPathUrl())
                            {
                                return string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                iconUrl = string.Empty;
            }

            return iconUrl;
        }
    }
}
