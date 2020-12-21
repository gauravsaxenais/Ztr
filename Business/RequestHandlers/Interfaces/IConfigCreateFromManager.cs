namespace Business.RequestHandlers.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// This module takes config.toml as input 
    /// and generates a model for the config.toml.
    /// </summary>
    public interface IConfigCreateFromManager
    {
        /// <summary>
        /// Gets the default values all modules asynchronous.
        /// </summary>
        /// <param name="configTomlFile">The firmware version.</param>
        /// <returns></returns>
        Task<string> GenerateConfigTomlModelAsync(string configTomlFile);
    }
}
