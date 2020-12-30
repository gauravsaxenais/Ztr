namespace Business.Parsers.ProtoParser.Models
{
    using System.Collections.Generic;

    public class JsonField : Field
    {
        public bool IsVisible { get; set; }
        public List<JsonField> Fields { get; private set; } = new List<JsonField>();
        public List<List<JsonField>> Arrays { get; private set; } = new List<List<JsonField>>();
    }
}
