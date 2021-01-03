namespace Business.Parsers.ProtoParser.Parser
{
    using Models;
    using System.Collections.Generic;

    public interface IModuleParser
    {
        List<JsonField> MergeTomlWithProtoMessage<T>(T configValues, ProtoParsedMessage protoParsedMessage)
            where T : Dictionary<string, object>;
    }
}
