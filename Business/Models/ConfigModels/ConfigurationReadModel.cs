namespace Business.Models.ConfigModels
{
    using System.Collections.Generic;
    public class ConfigurationReadModel
    {
        public List<ModuleReadModel> Module { get; set; } = new List<ModuleReadModel>();
        public List<NetworkReadModel> Network { get; set; } = new List<NetworkReadModel>();

        public override string ToString()
        {
            return $"RootReadModel(${this.Module} {this.Network})";
        }
    }
}
