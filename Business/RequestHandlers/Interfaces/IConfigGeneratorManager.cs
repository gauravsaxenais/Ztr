namespace Business.RequestHandlers.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IConfigGeneratorManager
    {
        Task<string> CreateConfigAsync(string jsonContent);
    }
}
