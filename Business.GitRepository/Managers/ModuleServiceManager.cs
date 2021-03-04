namespace Business.GitRepository.Managers
{
    using Business.Common.Models;
    using Common.Configuration;
    using EnsureThat;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Nett;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    /// <summary>
    /// Wrapper for GitRepoManager.
    /// </summary>
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleServiceManager" />
    /// <seealso cref="Manager" />
    /// <seealso cref="IModuleServiceManager" />
    public class ModuleServiceManager : ServiceManager, IModuleServiceManager
    {
        private readonly string protoFileName = "module.proto";
        private readonly ILogger<ModuleServiceManager> _logger;
        private const string Prefix = nameof(ModuleServiceManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="moduleGitConnectionOptions">The module git connection options.</param>
        /// <param name="gitRepoManager">The git repo manager.</param>
        /// <param name="firmwareVersionServiceManager">The firmware version service manager.</param>
        public ModuleServiceManager(ILogger<ModuleServiceManager> logger, ModuleGitConnectionOptions moduleGitConnectionOptions, IGitRepositoryManager gitRepoManager) : base(logger, moduleGitConnectionOptions, gitRepoManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            _logger = logger;
        }

        /// <summary>
        /// Gets all modules asynchronous.
        /// </summary>
        /// <param name="listOfModules">The list of modules.</param>
        public async Task UpdateMetaTomlAsync(List<ModuleReadModel> listOfModules)
        {
            string metaTomlFilePath = ((ModuleGitConnectionOptions)ConnectionOptions).MetaToml;
            string moduleFilePath = ((ModuleGitConnectionOptions)ConnectionOptions).GitLocalFolder;

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
            SetConnection((ModuleGitConnectionOptions)ConnectionOptions);
            await CloneGitHubRepoAsync().ConfigureAwait(false);
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

            string moduleFilePath = ((ModuleGitConnectionOptions)ConnectionOptions).GitLocalFolder;
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
