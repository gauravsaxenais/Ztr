namespace Business.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Compatible Firmware version Information.
    /// </summary>
    public class CompatibleFirmwareVersionReadModel
    {
        /// <summary>
        /// Gets or sets the firmware version.
        /// </summary>
        /// <value>
        /// The firmware version.
        /// </value>
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        /// <value>
        /// The type of the device.
        /// </value>
        public string DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the modules.
        /// </summary>
        /// <value>
        /// The modules.
        /// </value>
        public List<ModuleReadModel> Modules { get; set; } = new List<ModuleReadModel>();
    }
}
