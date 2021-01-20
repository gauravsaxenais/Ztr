namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;

    /// <summary>
    /// ManagerResponseTyped class.
    /// </summary>
    /// <typeparam name="TErrorCode">The type of the error code.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="ZTR.Framework.Business.ManagerResponseBase{TErrorCode}" />
    public class ManagerResponseTyped<TErrorCode, TResult> : ManagerResponseBase<TErrorCode>
        where TErrorCode : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseTyped{TErrorCode, TResult}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public ManagerResponseTyped(TErrorCode errorCode, string message)
            : base(errorCode, message)
        {
            Results = Array.Empty<TResult>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseTyped{TErrorCode, TResult}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The exception.</param>
        public ManagerResponseTyped(TErrorCode errorCode, Exception exception)
        : base(errorCode, exception)
        {
            Results = Array.Empty<TResult>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseTyped{TErrorCode, TResult}"/> class.
        /// </summary>
        /// <param name="errorRecords">The error records.</param>
        public ManagerResponseTyped(ErrorRecords<TErrorCode> errorRecords)
            : base(errorRecords)
        {
            Results = Array.Empty<TResult>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseTyped{TErrorCode, TResult}"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="results">The results.</param>
        public ManagerResponseTyped(TResult result, params TResult[] results)
        {
            EnsureArg.IsTrue(result != null, nameof(result));

            Results = results.Prepend(result).ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseTyped{TErrorCode, TResult}"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        public ManagerResponseTyped(IEnumerable<TResult> results)
            : base()
        {
            EnsureArg.IsNotNull(results, nameof(results));

            Results = results.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponseTyped{TErrorCode, TResult}"/> class.
        /// </summary>
        public ManagerResponseTyped()
            : base()
        {
            Results = Array.Empty<TResult>();
        }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public TResult[] Results { get; set; }
    }
}
