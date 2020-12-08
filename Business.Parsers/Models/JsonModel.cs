namespace Business.Parsers.Models
{
    using System.Collections.Generic;

    public class JsonModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public List<object> Arrays { get; set; } = new List<object>();
    }
}
