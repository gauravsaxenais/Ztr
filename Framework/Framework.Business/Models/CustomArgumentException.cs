namespace ZTR.Framework.Business.Models
{
    using System;

    public interface IApplicationException
    {
    }

    public class CustomArgumentException : ApplicationException, IApplicationException
    {
        public CustomArgumentException(string message) : base(message)
        { }

        public CustomArgumentException(string message, Exception exception) : base(message, exception)
        { }
    }
}
