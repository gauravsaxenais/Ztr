namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.Parsers;
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using Google.Protobuf;
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;
    using ZTR.Framework.Business.File.FileReaders;

    public class DefaultValueManager : Manager, IDefaultValueManager
    {
        private readonly IGitRepositoryManager _gitRepoManager;
        private readonly DeviceGitConnectionOptions _deviceGitConnectionOptions;

        public DefaultValueManager(ILogger<ModuleManager> logger, IGitRepositoryManager gitRepoManager, DeviceGitConnectionOptions deviceGitConnectionOptions) : base(logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(deviceGitConnectionOptions, nameof(deviceGitConnectionOptions));

            _gitRepoManager = gitRepoManager;
            _deviceGitConnectionOptions = deviceGitConnectionOptions;

            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _deviceGitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, _deviceGitConnectionOptions.GitLocalFolder);

            _gitRepoManager.SetConnectionOptions(_deviceGitConnectionOptions);
        }

        public async Task<string> GetDefaultValuesAllModulesAsync(string firmwareVersion, string deviceType)
        {
            var outputFolderPath = @"ProtoFiles\GeneratedFiles\";
            var protoFilePath = @"ProtoFiles\Proto";

            var json = string.Empty;

            var inputFileLoader = new InputFileLoader();
            var moduleParser = new ModuleParser();
            var defaultValueParser = new DefaultValueParser();

            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();

            var defaultFileContent = await GetDefaultData(firmwareVersion, deviceType);
            protoFilePath = inputFileLoader.CombinePathFromAppRoot(protoFilePath);

            foreach (string file in Directory.EnumerateFiles(protoFilePath, "*.proto"))
            {
                var fileName = Path.GetFileName(file);
                inputFileLoader.GenerateFiles(fileName, outputFolderPath, protoFilePath);
            }

            var messages = moduleParser.GetAllMessages();

            foreach (IMessage message in messages)
            {
                var formattedMessage = moduleParser.Format(message);

                json += defaultValueParser.ReadFileAsJson(defaultFileContent, tomlSettings, formattedMessage);
            }

            return json;
        }

        private async Task<string> GetDefaultData(string firmwareVersion, string deviceType)
        {
            var gitConnectionOptions = (DeviceGitConnectionOptions)_gitRepoManager.GetConnectionOptions();

            var listOfFiles = await _gitRepoManager.GetFileDataFromTagAsync(firmwareVersion, gitConnectionOptions.TomlConfiguration.DefaultTomlFile);

            // case insensitive search.
            var deviceTypeFile = listOfFiles.Where(p => p.FileName?.IndexOf(deviceType, StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();

            var fileContent = string.Empty;

            if (deviceTypeFile != null)
            {
                fileContent = System.Text.Encoding.UTF8.GetString(deviceTypeFile.Data);
            }

            return fileContent;
        }
    }
}
