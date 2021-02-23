namespace Business.Common.Models
{
    /// <summary>
    /// This class maps to toml file.
    /// The data in the toml file is
    /// mapped to the fields.
    /// </summary>
    public class ModuleReadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleReadModel"/> class.
        /// </summary>
        public ModuleReadModel()
        {}

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the UUID.
        /// </summary>
        /// <value>
        /// The UUID.
        /// </value>
        public string UUID { get; set; }

        /// <summary>
        /// Gets or sets the icon URL.
        /// </summary>
        /// <value>
        /// The icon URL.
        /// </value>
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public object Config { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"ModuleReadModel($ {Id} {Name} {UUID} {IconUrl} {Config})";
        }
    }
}
