namespace Business.GitRepository.Managers
{
    using Interfaces;

    public class GitRepository : IGitRepository
    {
        public string Id { get; set; }
        public string LocalLocation { get; set; }
        public string RemoteLocation { get; set; }

        //parameter less constructor for serialization
        public GitRepository() { }

        public GitRepository(string id, string localDirectory, string remotePathOrUrl)
        {
            Id = id;
            LocalLocation = localDirectory;
            RemoteLocation = remotePathOrUrl;
        }
    }
}
