namespace Business.Parser.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class Message
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public bool IsRepeated { get; set; }
        [JsonProperty("fields")]
        public List<Field> Fields { get; } = new List<Field>();

        [JsonProperty("messages")]
        public List<Message> Messages { get; } = new List<Message>();
    }
}
