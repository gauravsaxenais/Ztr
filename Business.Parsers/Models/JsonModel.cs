namespace Business.Parsers.Models
{
    using System.Collections.Generic;

    public class JsonModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public object Min { get; set; }

        public object Max { get; set; }

        public object DefaultValue { get; set; }

        public string DataType { get; set; }
        public List<JsonModel> Fields { get; private set; } = new List<JsonModel>();
        public List<List<JsonModel>> Arrays { get; private set; } = new List<List<JsonModel>>();
    }
}
