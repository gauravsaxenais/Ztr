namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ErrorRecords class.
    /// </summary>
    /// <typeparam name="TErrorCode">The type of the error code.</typeparam>
    /// <seealso cref="ErrorRecord{TErrorCode}" />
    public sealed class ErrorRecords<TErrorCode> : WrapperObject<ErrorRecord<TErrorCode>>
        where TErrorCode : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecords{TErrorCode}"/> class.
        /// </summary>
        public ErrorRecords()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecords{TErrorCode}"/> class.
        /// </summary>
        /// <param name="models">The models.</param>
        public ErrorRecords(IEnumerable<ErrorRecord<TErrorCode>> models)
            : base(models)
        {
        }
    }
}
