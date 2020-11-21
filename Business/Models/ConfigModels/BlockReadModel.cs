namespace Business.Models.ConfigModels
{
    public class BlockReadModel
    {
        public string Type { get; set; }
        public object Args { get; set; }

        public override string ToString()
        {
            return $"BlockReadModel(${this.Type} {this.Args})";
        }
    }
}
