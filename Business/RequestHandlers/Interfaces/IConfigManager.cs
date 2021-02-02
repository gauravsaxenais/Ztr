namespace Business.RequestHandlers.Interfaces
{
    using Business.Parsers.Core.Models;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Config Generator interface methods.
    /// </summary>
    public interface IConfigManager
    {
        /// <summary>
        /// Creates the configuration asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        Task<string> CreateConfigAsync(ConfigReadModel model);

        /// <summary>
        /// Updates the toml configuration.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        Task<bool> UpdateTomlConfig(string properties);

        /// <summary>
        /// Creates from HTML asynchronous.
        /// </summary>
        /// <param name="htmlFile">The HTML file.</param>
        /// <returns></returns>
        Task<string> CreateFromHtmlAsync(IFormFile htmlFile);
    }
}
