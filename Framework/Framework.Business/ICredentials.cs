namespace ZTR.Framework.Business
{
    /// <summary>
    /// Credentials password.
    /// </summary>
    /// <typeparam name="TPassword">The type of the password.</typeparam>
    public interface ICredentials<TPassword>
    {
        string Username { get; set; }
        TPassword Password { get; set; }
    }

    /// <summary>
    /// Credentials base.
    /// </summary>
    /// <typeparam name="TPassword">The type of the password.</typeparam>
    /// <seealso cref="ZTR.Framework.Business.ICredentials{TPassword}" />
    public abstract class CredentialsBase<TPassword> : ICredentials<TPassword>
    {
        public string Username { get; set; }
        public TPassword Password { get; set; }

        protected CredentialsBase(string username, TPassword password)
        {
            Username = username;
            Password = password;
        }
    }
}
