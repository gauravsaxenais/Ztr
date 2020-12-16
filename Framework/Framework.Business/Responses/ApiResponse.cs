using Newtonsoft.Json;

namespace ZTR.Framework.Business
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public ErrorMessage<ErrorType> Error { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
