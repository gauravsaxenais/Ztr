namespace Business.Models.ConfigModels
{
    using System.Collections.Generic;
    using System.Linq;

    public class NetworkReadModel
    {
        public List<BlockReadModel> Blocks { get; set; } = new List<BlockReadModel>();
        
        public override string ToString()
        {
            return $"NetworkReadModel({string.Join(",", this.Blocks.Select(p => p.ToString()))})";
        }
    }
}
