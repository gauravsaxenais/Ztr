namespace ZTR.Framework.Business
{
    /// <summary>
    /// Credentials password.
    /// </summary>
    /// <typeparam name="TPassword">The type of the password.</typeparam>
    public interface ICredentials<TPassword>
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        TPassword Password { get; set; }
    }

    /// <summary>
    /// Credentials base.
    /// </summary>
    /// <typeparam name="TPassword">The type of the password.</typeparam>
    /// <seealso cref="ZTR.Framework.Business.ICredentials{TPassword}" />
    public abstract class CredentialsBase<TPassword> : ICredentials<TPassword>
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public TPassword Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialsBase{TPassword}"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        protected CredentialsBase(string username, TPassword password)
        {
            Username = username;
            Password = password;
        }
    }
}
