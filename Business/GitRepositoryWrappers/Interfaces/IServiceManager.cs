namespace Business.GitRepositoryWrappers.Interfaces
{
    using ZTR.Framework.Business;

    /// <summary>
    /// Base class for Service Managers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IManager" />
    public interface IServiceManager<T> where T : GitConnectionOptions
    {
        /// <summary>
        /// Sets the git repo connection.
        /// </summary>
        public void SetGitRepoConnection();
    }
}
