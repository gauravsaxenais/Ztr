namespace Business.Parser.Models
{
    using Newtonsoft.Json;

    public class Field
    {
        [JsonProperty("name")]

        public string Name { get; set; }
        [JsonProperty("value")]

        public object Value { get; set; }
        [JsonProperty("min")]

        public object Min { get; set; }
        [JsonProperty("max")]

        public object Max { get; set; }
        [JsonProperty("datatype")]

        public string DataType { get; set; }
    }
}