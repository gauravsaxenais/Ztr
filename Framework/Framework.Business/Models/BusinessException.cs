using System;
using System.Collections.Generic;
using System.Text;

namespace ZTR.Framework.Business.Models
{
    public class BusinessException : ApplicationException, IApplicationException
    {
        public BusinessException(string message) :base(message)
        { }

        public BusinessException(string message, Exception exception) : base(message, exception)
        { }
    }
}
