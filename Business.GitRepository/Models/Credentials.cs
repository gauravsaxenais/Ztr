namespace Business.GitRepository.Models
{
    using System.Security;
    using ZTR.Framework.Business;

    /// <summary>
    /// Stores user name and password credentials.
    /// </summary>
    /// <remarks>
    /// Do no use internally. For COM Interop only. Use <see cref="SecureCredentials"/> instead./>
    /// </remarks>
    public class Credentials : CredentialsBase<string>
    {
        public Credentials(string username, string password)
            : base(username, password)
        { }
    }

    /// <summary>
    /// Securely stores user name and password credentials.
    /// </summary>
    public class SecureCredentials : CredentialsBase<SecureString>
    {
        public SecureCredentials(string username, SecureString password)
            : base(username, password)
        { }
    }
}
