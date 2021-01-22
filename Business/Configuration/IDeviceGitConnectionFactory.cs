namespace Business.Configuration
{
    using ZTR.Framework.Business;

    /// <summary>
    /// A factory for managing connections.
    /// </summary>
    public interface IDeviceGitConnectionFactory
    {
        /// <summary>
        /// Gets the device connection.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        /// <returns></returns>
        GitConnectionOptions GetDeviceConnection(ConnectionType connectionType);
    }
}
