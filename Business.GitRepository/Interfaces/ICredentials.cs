namespace Business.GitRepository.Interfaces
{
    public interface ICredentials<TPassword>
    {
        string Username { get; set; }
        TPassword Password { get; set; }
    }

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
