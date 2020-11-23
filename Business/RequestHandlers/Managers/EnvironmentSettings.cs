namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using EnsureThat;
    using System.IO;

    public class EnvironmentSettings : IEnvironmentSettings
    {
        private readonly DeviceGitConnectionOptions gitConnectionOptions;

        public EnvironmentSettings(DeviceGitConnectionOptions gitConnectionOptions)
        {
            EnsureArg.IsNotNull(gitConnectionOptions, nameof(gitConnectionOptions));
            EnsureArg.IsNotNull(gitConnectionOptions.TomlConfiguration, nameof(gitConnectionOptions.TomlConfiguration));

            this.gitConnectionOptions = gitConnectionOptions;
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            gitConnectionOptions.GitLocalFolder = Path.Combine(currentDirectory, gitConnectionOptions.GitLocalFolder);
            gitConnectionOptions.TomlConfiguration.DeviceFolder = Path.Combine(gitConnectionOptions.GitLocalFolder, gitConnectionOptions.TomlConfiguration.DeviceFolder);
        }

        public DeviceGitConnectionOptions GetDeviceGitConnectionOptions()
        {
            return gitConnectionOptions;
        }
    }
}
