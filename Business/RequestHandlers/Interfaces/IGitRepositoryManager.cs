namespace Business.RequestHandlers.Interfaces
{
    using Business.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZTR.Framework.Business.File;

    public interface IGitRepositoryManager
    {
        void SetConnectionOptions(GitConnectionOptions gitConnection);
        GitConnectionOptions GetConnectionOptions();
        Task CloneRepositoryAsync();
        Task<string[]> LoadTagNamesAsync();
        Task<IEnumerable<ExportFileResultModel>> GetFileDataFromTagAsync(string tag, string fileName);
    }
}
