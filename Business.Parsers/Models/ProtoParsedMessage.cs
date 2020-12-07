namespace Business.Parsers.Models
{
    using Business.Parsers.Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class ProtoParsedMessage
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public bool IsRepeated { get; set; }

        public List<Field> Fields { get; } = new List<Field>();
        
        public List<ProtoParsedMessage> Messages { get; } = new List<ProtoParsedMessage>();
    }
}
