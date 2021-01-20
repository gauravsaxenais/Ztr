namespace ZTR.Framework.Business
{
    using System;
    using System.Linq;
    using EnsureThat;

    /// <summary>
    /// ManagerResponseBase class.
    /// </summary>
    /// <typeparam name="TErrorCode">The type of the error code.</typeparam>
    public abstract class ManagerResponseBase<TErrorCode>
        where TErrorCode : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseBase{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public ManagerResponseBase(TErrorCode errorCode, string message)
        {
            EnsureArg.IsNotNull<TErrorCode>(errorCode, nameof(errorCode));
            EnsureArg.IsNotEmptyOrWhiteSpace(message, nameof(message));

            ErrorRecords = new ErrorRecords<TErrorCode>
            {
                new ErrorRecord<TErrorCode>(errorCode, message)
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseBase{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The exception.</param>
        public ManagerResponseBase(TErrorCode errorCode, Exception exception)
        {
            EnsureArg.IsNotNull<TErrorCode>(errorCode, nameof(errorCode));
            EnsureArg.IsNotNull(exception, nameof(exception));

            ErrorRecords = new ErrorRecords<TErrorCode>
            {
                new ErrorRecord<TErrorCode>(errorCode, exception)
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseBase{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorRecords">The error records.</param>
        public ManagerResponseBase(ErrorRecords<TErrorCode> errorRecords)
        {
            EnsureArg.IsNotNull(errorRecords, nameof(errorRecords));

            ErrorRecords = errorRecords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseBase{TErrorCode}"/> class.
        /// </summary>
        protected ManagerResponseBase()
        {
            ErrorRecords = new ErrorRecords<TErrorCode>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has an error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has an error; otherwise, <c>false</c>.
        /// </value>
        public bool HasError
        {
            get { return ErrorRecords.Any(); }
        }

        /// <summary>
        /// Gets the error records.
        /// </summary>
        /// <value>
        /// The error records.
        /// </value>
        public ErrorRecords<TErrorCode> ErrorRecords { get; set; }
    }
}
