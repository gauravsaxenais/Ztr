namespace Business.GitRepository.Interfaces
{
    using System.ComponentModel;

    public interface IGitRepository
    {
        string Id { get; }

        [Description("FilePath of local repository.")]
        string LocalLocation { get; }

        [Description("FilePath or URL of remote repository.")]
        string RemoteLocation { get; }
    }
}
