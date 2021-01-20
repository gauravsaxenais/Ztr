namespace ZTR.Framework.Business.Models
{
    using System;

    /// <summary>
    /// Exception class for sending business related messages.
    /// </summary>
    /// <seealso cref="System.ApplicationException" />
    /// <seealso cref="ZTR.Framework.Business.Models.IApplicationException" />
    public class BusinessException : ApplicationException, IApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public BusinessException(string message) : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public BusinessException(string message, Exception exception) : base(message, exception)
        { }
    }
}
