namespace Business.Parsers.Models
{
    using System;

    public class Field : ICloneable
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }
        
        public object Min { get; set; }
        
        public object Max { get; set; }

        public object DefaultValue { get; set; }

        public string DataType { get; set; }

        public object Clone()
        {
            Field other = (Field)this.MemberwiseClone();

            DeepCopy(other);

            return other;
        }

        private void DeepCopy(Field other)
        {
            other.Name = Name;
            other.Value = Value;
            other.Min = Min;
            other.Max = Max;
            other.DefaultValue = DefaultValue;
            other.DataType = DataType;
        }
    }
}