namespace Business.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Block - This class maps to toml file.
    /// </summary>
    public class BlockReadModel
    {
        /// <summary>
        /// Gets or sets the network arguments.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public List<NetworkArgumentReadModel> Arguments { get; set; } = new List<NetworkArgumentReadModel>();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"BlockReadModel(Arguments: {string.Join(",", this.Arguments.Select(p => p.ToString()))})";
        }
    }
}
