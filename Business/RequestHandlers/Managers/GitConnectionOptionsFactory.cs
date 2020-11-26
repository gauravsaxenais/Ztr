namespace Business.RequestHandlers.Managers
{
    using Business.Configuration;
    using Business.RequestHandlers.Interfaces;
    using System;
    using ZTR.Framework.Business.File;

    public class GitConnectionOptionsFactory : IGitConnectionOptionsFactory
    {
        public GitConnectionOptions GetGitConnectionOption(GitConnectionOptionType connectionType)
        {
            return connectionType switch
            {
                GitConnectionOptionType.Module => (ModuleGitConnectionOptions)Activator.CreateInstance(Type.GetType(typeof(ModuleGitConnectionOptions).AssemblyQualifiedName, false)),
                GitConnectionOptionType.Device => (DeviceGitConnectionOptions)Activator.CreateInstance(Type.GetType(typeof(DeviceGitConnectionOptions).AssemblyQualifiedName, false)),
                GitConnectionOptionType.Block => (BlockGitConnectionOptions)Activator.CreateInstance(Type.GetType(typeof(BlockGitConnectionOptions).AssemblyQualifiedName, false)),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
