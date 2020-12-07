namespace Business.Parsers.Models
{
    using Business.Parsers.Models;
    using System.Collections.Generic;

    public class JsonModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public object Arrays { get; set; }
    }
}
