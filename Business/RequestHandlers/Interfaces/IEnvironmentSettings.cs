namespace Business.RequestHandlers.Interfaces
{
    using Business.Configuration;
    public interface IEnvironmentSettings
    {
        public DeviceGitConnectionOptions GetDeviceGitConnectionOptions();
    }
}
