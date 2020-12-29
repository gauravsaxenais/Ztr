namespace Business.Parsers.ProtoParser.Models
{
    using Google.Protobuf;

    public class CustomMessage
    {
        public string Name { get; set; }
        public IMessage Message { get; set; }
    }
}
