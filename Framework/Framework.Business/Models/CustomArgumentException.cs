using System;
using System.Collections.Generic;
using System.Text;

namespace ZTR.Framework.Business.Models
{
    public interface IApplicationException
    {

    }
    public class CustomArgumentException : ApplicationException, IApplicationException
    {
        public CustomArgumentException(string message) :base(message)
        { }

        public CustomArgumentException(string message, Exception exception) : base(message, exception)
        { }
    }
}
