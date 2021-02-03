namespace Business.RequestHandlers.Interfaces
{
    using Microsoft.AspNetCore.Http;
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
        Task<object> GenerateConfigTomlModelAsync(IFormFile configTomlFile);

        /// <summary>
        /// Gets the default values all modules asynchronous.
        /// </summary>
        /// <param name="configTomlFileContent">The firmware version.</param>
        /// <returns></returns>
        Task<object> GenerateConfigTomlModelAsync(string configTomlFileContent);

        /// <summary>
        /// Generates the configuration toml model without git asynchronous.
        /// </summary>
        /// <param name="configTomlFileContent">Content of the configuration toml file.</param>
        /// <returns></returns>
        Task<object> GenerateConfigTomlModelWithoutGitAsync(string configTomlFileContent);
    }
}
