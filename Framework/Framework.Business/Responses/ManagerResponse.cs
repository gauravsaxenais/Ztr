namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;

    /// <summary>
    /// Manager responseclass.
    /// </summary>
    /// <typeparam name="TErrorCode">The type of the error code.</typeparam>
    /// <seealso cref="ZTR.Framework.Business.ManagerResponseBase{TErrorCode}" />
    public class ManagerResponse<TErrorCode> : ManagerResponseBase<TErrorCode>
        where TErrorCode : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponse{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public ManagerResponse(TErrorCode errorCode, string message)
            : base(errorCode, message)
        {
            Ids = Array.Empty<long>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponse{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The exception.</param>
        public ManagerResponse(TErrorCode errorCode, Exception exception)
            : base(errorCode, exception)
        {
            Ids = Array.Empty<long>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponse{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorRecords">The error records.</param>
        public ManagerResponse(ErrorRecords<TErrorCode> errorRecords)
            : base(errorRecords)
        {
            Ids = Array.Empty<long>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponse{TErrorCode}"/> class.
        /// </summary>
        /// <param name="ids">The ids.</param>
        public ManagerResponse(IEnumerable<long> ids)
            : base()
        {
            EnsureArg.IsNotNull(ids, nameof(ids));

            Ids = ids.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerResponse{TErrorCode}"/> class.
        /// </summary>
        public ManagerResponse()
            : base()
        {
            Ids = Array.Empty<long>();
        }

        /// <summary>
        /// Gets or sets the ids.
        /// </summary>
        /// <value>
        /// The ids.
        /// </value>
        public long[] Ids { get; set; }
    }
}
