namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;

    public class ManagerResponse<TErrorCode> : ManagerResponseBase<TErrorCode>
        where TErrorCode : struct, Enum
    {
        public ManagerResponse(TErrorCode errorCode, string message)
            : base(errorCode, message)
        {
            Ids = Array.Empty<long>();
        }

        public ManagerResponse(TErrorCode errorCode, Exception exception)
            : base(errorCode, exception)
        {
            Ids = Array.Empty<long>();
        }

        public ManagerResponse(ErrorRecords<TErrorCode> errorRecords)
            : base(errorRecords)
        {
            Ids = Array.Empty<long>();
        }

        public ManagerResponse(IEnumerable<long> ids)
            : base()
        {
            EnsureArg.IsNotNull(ids, nameof(ids));

            Ids = ids.ToArray();
        }

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
