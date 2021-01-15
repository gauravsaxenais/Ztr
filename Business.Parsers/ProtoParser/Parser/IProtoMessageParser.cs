namespace Business.Parsers.ProtoParser.Parser
{
    using Models;
    using System.Threading.Tasks;

    public interface IProtoMessageParser
    {
        /// <summary>
        /// Gets the custom messages.
        /// </summary>
        /// <param name="protoFilePath">The proto file path.</param>
        /// <returns>custom message containing the proto parsed message</returns>
        Task<CustomMessage> GetCustomMessage(string protoFilePath);
    }
}
