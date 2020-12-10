namespace Business.Models
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
        /// This identifier starts from 0 and
        /// autoincrements for every new model.
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
            return $"ModuleReadModel(${this.Name} {this.UUID} {this.Config})";
        }
    }
}
