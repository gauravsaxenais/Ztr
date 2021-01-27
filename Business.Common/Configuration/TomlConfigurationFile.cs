namespace Business.Common.Configuration
{
    /// <summary>
    /// Toml configuration file. 
    /// </summary>
    public class TomlConfigurationFile
    {
        /// <summary>
        /// Gets or sets the device folder.
        /// </summary>
        /// <value>
        /// The device folder.
        /// </value>
        public string DeviceFolder { get; set; }

        /// <summary>
        /// Gets or sets the toml configuration folder.
        /// </summary>
        /// <value>
        /// The toml configuration folder.
        /// </value>
        public string TomlConfigFolder { get; set; }

        /// <summary>
        /// Gets or sets the device toml file.
        /// </summary>
        /// <value>
        /// The device toml file.
        /// </value>
        public string DeviceTomlFile { get; set; }

        /// <summary>
        /// Gets or sets the default toml file.
        /// </summary>
        /// <value>
        /// The default toml file.
        /// </value>
        public string DefaultTomlFile { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"TomlConfigurationFile($ {this.DeviceFolder} {this.TomlConfigFolder} {this.DeviceTomlFile} {this.DeviceFolder})";
        }
    }
}
