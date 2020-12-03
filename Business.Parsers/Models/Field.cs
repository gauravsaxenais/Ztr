namespace Business.Parser.Models
{
    using System;
    public class Field : ICloneable
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }
        
        public object Min { get; set; }
        
        public object Max { get; set; }
        
        public string DataType { get; set; }

        public Field Clone()
        {
            var newField = new Field
            {
                Id = this.Id,
                Name = this.Name,
                Value = this.Value,
                Min = this.Min,
                Max = this.Max,
                DataType = this.DataType
            };

            return newField;
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }
}