namespace Business.Parsers.Models
{
    using Google.Protobuf;

    public class CustomIMessage
    {
        public string Name { get; set; }
        public IMessage Message { get; set; }
    }
}
