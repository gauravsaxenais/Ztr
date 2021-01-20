namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TErrorCode">The type of the error code.</typeparam>
    /// <seealso cref="ErrorMessage{TErrorCode}" />
    public class ErrorMessages<TErrorCode> : WrapperObject<ErrorMessage<TErrorCode>>
        where TErrorCode : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessages{TErrorCode}"/> class.
        /// </summary>
        public ErrorMessages()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessages{TErrorCode}"/> class.
        /// </summary>
        /// <param name="models">The models.</param>
        public ErrorMessages(IEnumerable<ErrorMessage<TErrorCode>> models)
            : base(models)
        {
        }
    }
}
