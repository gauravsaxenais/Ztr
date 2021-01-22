namespace Business.Configuration
{
    using ZTR.Framework.Business;

    /// <summary>
    /// An implementation for git connection
    /// from appsettings.json
    /// </summary>
    public class DeviceGitConnectionFactory : IDeviceGitConnectionFactory
    {
        /// <summary>
        /// Gets the device connection.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public GitConnectionOptions GetDeviceConnection(ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case ConnectionType.Device: return new ModuleBlockGitConnectionOptions();
                case ConnectionType.Devices: return new DeviceGitConnectionOptions();
                case ConnectionType.Firmware: return new FirmwareVersionGitConnectionOptions();
                default: return null;
            }
        }
    }
}
