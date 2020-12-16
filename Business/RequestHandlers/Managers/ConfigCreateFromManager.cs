namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    public class ConfigCreateFromManager : Manager, IConfigCreateFromManager
    {
        /// <summary>Initializes a new instance of the <see cref="ConfigCreateFromManager" /> class.</summary>
        /// <param name="logger">The logger.</param>
        public ConfigCreateFromManager(ILogger<ConfigCreateFromManager> logger) : base(logger) { }

        public async Task GenerateConfigTomlModelAsync(string configTomlFile)
        {
            
        }
    }
}
