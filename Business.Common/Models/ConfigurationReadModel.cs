namespace Business.Common.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ConfigurationReadModel - This class maps to toml file.
    /// </summary>
    public class ConfigurationReadModel
    {
        /// <summary>
        /// Gets or sets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public List<ModuleReadModel> Module { get; set; } = new List<ModuleReadModel>();

        /// <summary>
        /// Gets or sets the device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public DeviceReadModel Device { get; set; }

        /// <summary>
        /// Gets or sets the network.
        /// </summary>
        /// <value>
        /// The network.
        /// </value>
        public Dictionary<string, object> Network { get; set; } =
            new Dictionary<string, object>();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"ConfigurationReadModel({this.Device} {string.Join(",", this.Module.Select(p => p.ToString()))})";
        }
    }
}
