namespace Business.Parsers.Models
{
    using Business.Parser.Models;
    using Business.Parsers.Models.JsonModels;
    using System.Collections.Generic;

    public class JsonConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public JsonArray Array { get; set; }
    }
}
