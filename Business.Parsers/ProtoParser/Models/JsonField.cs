namespace Business.Parsers.ProtoParser.Models
{
    using System.Collections.Generic;

    public class JsonField : Field
    {
        public bool IsFieldRepeated { get; set; }
        public List<JsonField> Fields { get; } = new List<JsonField>();
        public List<List<JsonField>> Arrays { get; } = new List<List<JsonField>>();
        public bool IsVisible { get; set; }
    }
}
