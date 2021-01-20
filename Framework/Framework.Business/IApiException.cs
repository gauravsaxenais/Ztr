namespace ZTR.Framework.Business
{
    using System.Collections.Generic;

    /// <summary>
    /// IApi Exception
    /// </summary>
    public interface IApiException
    {
        int StatusCode { get; }

        string Response { get; }

        IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }
    }
}
