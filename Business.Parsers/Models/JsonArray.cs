namespace Business.Parsers.Models
{
    using System.Collections.Generic;

    public class JsonArray
    {
        public string Name { get; set; }
        public bool IsRepeated { get; set; }
        public List<List<object>> Data { get; set; } = new List<List<object>>();
    }
}
