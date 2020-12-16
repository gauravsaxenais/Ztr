namespace Business.RequestHandlers.Interfaces
{
    using Business.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Config Generator interface methods.
    /// </summary>
    public interface IConfigGeneratorManager
    {
        /// <summary>Creates the configuration asynchronous.</summary>
        /// <param name="jsonContent">Content of the json.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        Task<string> CreateConfigAsync(ConfigModel jsonContent);

        Task<bool> UpdateTomlConfig(string properties);
    }
}
