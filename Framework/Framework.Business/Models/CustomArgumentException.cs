namespace ZTR.Framework.Business.Models
{
    using System;

    /// <summary>
    /// Application Exception class.
    /// </summary>
    public interface IApplicationException
    {
    }

    /// <summary>
    /// CustomArgument Exception class.
    /// </summary>
    /// <seealso cref="System.ArgumentException" />
    /// <seealso cref="ZTR.Framework.Business.Models.IApplicationException" />
    public class CustomArgumentException : ArgumentException, IApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomArgumentException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public CustomArgumentException(string message) : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public CustomArgumentException(string message, Exception exception) : base(message, exception)
        { }
    }
}
