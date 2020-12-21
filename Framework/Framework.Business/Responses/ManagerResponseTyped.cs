namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;

    public class ManagerResponseTyped<TErrorCode, TResult> : ManagerResponseBase<TErrorCode>
        where TErrorCode : struct, Enum
    {
        public ManagerResponseTyped(TErrorCode errorCode, string message)
            : base(errorCode, message)
        {
            Results = Array.Empty<TResult>();
        }

        public ManagerResponseTyped(TErrorCode errorCode, Exception exception)
        : base(errorCode, exception)
        {
            Results = Array.Empty<TResult>();
        }

        public ManagerResponseTyped(ErrorRecords<TErrorCode> errorRecords)
            : base(errorRecords)
        {
            Results = Array.Empty<TResult>();
        }

        public ManagerResponseTyped(TResult result, params TResult[] results)
        {
            EnsureArg.IsTrue(result != null, nameof(result));

            Results = results.Prepend(result).ToArray();
        }

        public ManagerResponseTyped(IEnumerable<TResult> results)
            : base()
        {
            EnsureArg.IsNotNull(results, nameof(results));

            Results = results.ToArray();
        }

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
