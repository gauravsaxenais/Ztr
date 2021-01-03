namespace Business.Parsers.ProtoParser.Parser
{
    using Models;
    using EnsureThat;
    using Google.Protobuf;
    using Google.Protobuf.Reflection;
    using System.Linq;

    public class CustomMessageParser : ICustomMessageParser
    {
        /// <summary>
        /// Formats the specified protoParsedMessage as JSON.
        /// </summary>
        /// <param name="message">The protoParsedMessage to format.</param>
        /// <returns>The formatted protoParsedMessage.</returns>
        public ProtoParsedMessage Format(IMessage message)
        {
            EnsureArg.IsNotNull(message);

            var protoParserMessage = new ProtoParsedMessage();
            Format(message, protoParserMessage);

            return protoParserMessage;
        }

        /// <summary>
        /// Formats the specified protoParsedMessage as Protoparser protoParsedMessage.
        /// </summary>
        /// <param name="message">The protoParsedMessage to format.</param>
        /// <param name="protoParserProtoParsedMessage">The field to parse the formatted protoParsedMessage to.</param>
        /// <returns>The formatted protoParsedMessage.</returns>
        public ProtoParsedMessage Format(IMessage message, ProtoParsedMessage protoParserProtoParsedMessage)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            EnsureArg.IsNotNull(protoParserProtoParsedMessage, nameof(protoParserProtoParsedMessage));

            ProcessMessageFields(protoParserProtoParsedMessage, message);
            return protoParserProtoParsedMessage;
        }

        private void ProcessMessageFields(ProtoParsedMessage protoParserProtoParsedMessage, IMessage message)
        {
            var fieldCollection = message.Descriptor.Fields.InFieldNumberOrder();
            for (var tempIndex = 0; tempIndex < fieldCollection.Count; tempIndex++)
            {
                if (fieldCollection[tempIndex].FieldType == FieldType.Message)
                {
                    var temp = new ProtoParsedMessage
                    {
                        Id = tempIndex,
                        Name = fieldCollection[tempIndex].Name,
                        IsRepeated = fieldCollection[tempIndex].IsRepeated
                    };

                    protoParserProtoParsedMessage.Messages.Add(temp);

                    IMessage cleanSubMessage = fieldCollection[tempIndex].MessageType.Parser.ParseFrom(ByteString.Empty);
                    ProcessMessageFields(temp, cleanSubMessage);
                }
                else
                {
                    var field = InitFieldValues(fieldCollection[tempIndex]);

                    if (field != null)
                    {
                        field.Id = tempIndex;
                        field.Name = fieldCollection[tempIndex].Name;

                        if (fieldCollection[tempIndex].FieldType == FieldType.Enum)
                        {
                            var firstValue = fieldCollection[tempIndex].EnumType.Values.First();
                            var lastValue = fieldCollection[tempIndex].EnumType.Values.Last();

                            field.Min = firstValue.Number;
                            field.Max = lastValue.Number;
                        }

                        protoParserProtoParsedMessage.Fields.Add(field);
                    }
                }
            }
        }

        private Field InitFieldValues(FieldDescriptor descriptor)
        {
            switch (descriptor.FieldType)
            {
                case FieldType.Bool:
                    return new Field() { DataType = "bool", DefaultValue = false, Min = 0, Max = 0, Value = false };
                case FieldType.Bytes:
                case FieldType.String:
                    return new Field() { DataType = "string", DefaultValue = string.Empty, Min = string.Empty, Max = string.Empty, Value = string.Empty };
                case FieldType.Double:
                    return new Field() { DataType = "double", DefaultValue = 0.0, Min = double.MinValue, Max = double.MaxValue, Value = 0.0 };
                case FieldType.SInt32:
                case FieldType.Int32:
                case FieldType.SFixed32:
                    return new Field() { DataType = "integer", DefaultValue = 0, Min = int.MinValue, Max = int.MaxValue, Value = 0 };
                case FieldType.Fixed32:
                case FieldType.UInt32:
                    return new Field() { DataType = "integer", DefaultValue = 0, Min = uint.MinValue, Max = uint.MaxValue, Value = 0 };
                case FieldType.Fixed64:
                case FieldType.UInt64:
                    return new Field() { DataType = "integer", DefaultValue = 0, Min = ulong.MinValue, Max = ulong.MaxValue, Value = 0 };
                case FieldType.SFixed64:
                case FieldType.Int64:
                case FieldType.SInt64:
                    return new Field() { DataType = "integer", DefaultValue = 0, Min = long.MinValue, Max = long.MaxValue, Value = 0 };
                case FieldType.Enum:
                    return new Field() { DataType = "enum", DefaultValue = 0, Min = 0, Max = 0, Value = 0 };
                case FieldType.Float:
                    return new Field() { DataType = "float", DefaultValue = 0.0, Min = float.MinValue, Max = float.MaxValue, Value = 0.0 };
                default:
                    return null;
            }
        }
    }
}

