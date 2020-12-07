namespace Business.Models
{
    /// <summary>
    /// This class maps to toml file.
    /// Toml file has a device which has all the
    /// fields.
    /// </summary>
    public class DeviceReadModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public object Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public object Description { get; set; }

        /// <summary>
        /// Gets or sets the digital inputs.
        /// </summary>
        /// <value>
        /// The digital inputs.
        /// </value>
        public int Digital_Inputs { get; set; }

        /// <summary>
        /// Gets or sets the digital outputs.
        /// </summary>
        /// <value>
        /// The digital outputs.
        /// </value>
        public int Digital_Outputs { get; set; }

        /// <summary>
        /// Gets or sets the analog inputs.
        /// </summary>
        /// <value>
        /// The analog inputs.
        /// </value>
        public int Analog_Inputs { get; set; }

        /// <summary>
        /// Gets or sets the uarts.
        /// </summary>
        /// <value>
        /// The uarts.
        /// </value>
        public int Uarts { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"DeviceReadModel(${this.Name} {this.Description} {this.Digital_Inputs} {this.Digital_Outputs} {this.Analog_Inputs} {this.Uarts})";
        }
    }
}
