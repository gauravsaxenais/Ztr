namespace Business.Parsers.ProtoParser.Parser
{
    using Business.Parsers.ProtoParser.Models;
    using System.Threading.Tasks;

    public interface IProtoMessageParser
    {
        Task<CustomMessage> GetProtoParsedMessage(string moduleName, string protoFileName, string protoFilePath, params string[] args);
    }
}
