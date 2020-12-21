namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;

    public sealed class ErrorRecords<TErrorCode> : WrapperObject<ErrorRecord<TErrorCode>>
        where TErrorCode : struct, Enum
    {
        public ErrorRecords()
        {
        }

        public ErrorRecords(IEnumerable<ErrorRecord<TErrorCode>> models)
            : base(models)
        {
        }
    }
}
