using Business.Parser.Models;
using System.Collections.Generic;

namespace Business.Parsers.Models.JsonModels
{
    public class JsonArray
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public JsonArray Arrays { get; set; }
    }
}
