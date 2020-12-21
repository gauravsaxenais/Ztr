namespace ZTR.Framework.Business
{
    using System;
    using EnsureThat;
    using Newtonsoft.Json;

    public class ApiResponse
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public ErrorMessage<ErrorType> Error { get; set; }

        public ApiResponse() { }

        public ApiResponse(bool status, object data, ErrorType errorCode, Exception exception)
        {
            EnsureArg.IsNotNull(data, nameof(data));

            Success = status;
            Data = data;
            Error = new ErrorMessage<ErrorType>(errorCode, exception);
        }

        public ApiResponse(ErrorType errorCode, Exception exception) : this (false, exception, errorCode, exception)
        {
        }

        public ApiResponse(bool status, object data)
        {
            EnsureArg.IsNotNull(data, nameof(data));

            Success = status;
            Data = data;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
