namespace Business.Parsers.ProtoParser.Models
{
    using Newtonsoft.Json;
    public class Field
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public object Min { get; set; }        
        public object Max { get; set; }
        public object DefaultValue { get; set; }
        public string DataType { get; set; }
        [JsonIgnore]
        public bool IsRepeated { get; set; }
    }
}