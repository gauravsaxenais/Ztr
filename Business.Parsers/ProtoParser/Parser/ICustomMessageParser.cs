namespace Business.Parsers.ProtoParser.Parser
{
    using Models;
    using Google.Protobuf;

    public interface ICustomMessageParser
    {
        /// <summary>
        /// Formats the specified protoParsedMessage.
        /// </summary>
        /// <param name="message">The protoParsedMessage.</param>
        /// <returns>Proto parsed protoParsedMessage.</returns>
        ProtoParsedMessage Format(IMessage message);

        /// <summary>
        /// Formats the specified protoParsedMessage.
        /// </summary>
        /// <param name="message">The protoParsedMessage.</param>
        /// <param name="protoParserProtoParsedMessage">The proto parser protoParsedMessage.</param>
        /// <returns>Proto parsed protoParsedMessage.</returns>
        ProtoParsedMessage Format(IMessage message, ProtoParsedMessage protoParserProtoParsedMessage);
    }
}
