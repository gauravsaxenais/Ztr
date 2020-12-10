namespace Business.Models
{
    using Newtonsoft.Json;
    // name = "A_low", label = "12V Low", description = "12V min value.", datatype = "integer", min = "0", max = "200" },
    /// <summary>
    ///   <br />
    /// </summary>
    public class NetworkArgumentReadModel
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }
        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the label.</summary>
        /// <value>The label.</value>
        public string Label { get; set; }

        /// <summary>Gets or sets the description.</summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>Gets or sets the type of the data.</summary>
        /// <value>The type of the data.</value>
        [JsonProperty("datatype")]
        public string DataType { get; set; }

        /// <summary>Determines the minimum of the parameters.</summary>
        /// <value>The minimum.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Min { get; set; }

        /// <summary>Determines the maximum of the parameters.</summary>
        /// <value>The maximum.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Max { get; set; }
    }
}
