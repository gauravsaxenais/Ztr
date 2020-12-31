namespace Business.Parsers.ProtoParser.Parser
{
    using Business.Parsers.ProtoParser.Models;
    using System.Threading.Tasks;

    public interface IProtoMessageParser
    {
        /// <summary>
        /// Gets the proto parsed message.
        /// </summary>
        /// <param name="protoFileName">Name of the proto file.</param>
        /// <param name="protoFilePath">The proto file path.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        Task<CustomMessage> GetProtoParsedMessage(string protoFileName, string protoFilePath, params string[] args);
    }
}
