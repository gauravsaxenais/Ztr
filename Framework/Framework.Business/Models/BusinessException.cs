namespace ZTR.Framework.Business.Models
{
    using System;

    public class BusinessException : ApplicationException, IApplicationException
    {
        public BusinessException(string message) : base(message)
        { }

        public BusinessException(string message, Exception exception) : base(message, exception)
        { }
    }
}
