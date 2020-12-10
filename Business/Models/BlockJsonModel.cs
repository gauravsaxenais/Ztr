namespace Business.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///   Returns list of arguments for block as a json.
    /// </summary>
    public class BlockJsonModel
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }
        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public string Type { get; set; }
        /// <summary>Gets or sets the tag.</summary>
        /// <value>The tag.</value>
        public string Tag { get; set; }
        /// <summary>Gets the arguments.</summary>
        /// <value>The arguments.</value>
        public List<NetworkArgumentReadModel> Args { get; set; } = new List<NetworkArgumentReadModel>();
    }
}
