namespace Business.Parsers.Models
{
    public class Field
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }
        
        public object Min { get; set; }
        
        public object Max { get; set; }

        public object DefaultValue { get; set; }

        public string DataType { get; set; }

        public Field DeepCopy()
        {
            Field other = (Field)this.MemberwiseClone();
            other.Name = Name;
            other.Value = Value;
            other.Min = Min;
            other.Max = Max;
            other.DefaultValue = DefaultValue;
            other.DataType = DataType;

            return other;
        }
    }
}