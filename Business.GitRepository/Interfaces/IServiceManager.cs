namespace Business.GitRepository.Interfaces
{
    using ZTR.Framework.Business;
    using ZTR.Framework.Configuration;

    /// <summary>
    /// Base class for Service Managers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IManager" />
    public interface IServiceManager
    {
        void SetConnection(GitConnectionOptions connectionOptions);
    }
}
