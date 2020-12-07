namespace Business.Parsers
{
    using Business.Parsers.Models;
    using EnsureThat;
    using Google.Protobuf;
    using Google.Protobuf.Reflection;
    using System;
    using System.IO;
    using System.Linq;

    public class CustomMessageParser
    {
        /// <summary>
        /// Formats the specified message as JSON.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <returns>The formatted message.</returns>
        public ProtoParsedMessage Format(IMessage message)
        {
            EnsureArg.IsNotNull(message);

            var messageName = Path.GetFileNameWithoutExtension(message.Descriptor.File.Name);
            var protoParserMessage = new ProtoParsedMessage() { Name = messageName };

            Format(message, protoParserMessage);

            return protoParserMessage;
        }

        /// <summary>
        /// Formats the specified message as Protoparser message.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <param name="protoParserMessage">The field to parse the formatted message to.</param>
        /// <returns>The formatted message.</returns>
        public void Format(IMessage message, ProtoParsedMessage protoParserMessage)
        {
            ProtoPreconditions.CheckNotNull(message, nameof(message));

            WriteMessage(protoParserMessage, message);
        }

        private void WriteMessage(ProtoParsedMessage protoParserMessage, IMessage message)
        {
            if (message == null)
            {
                return;
            }

            WriteMessageFields(protoParserMessage, message);
        }

        private void WriteMessageFields(ProtoParsedMessage protoParserMessage, IMessage message)
        {
            var fieldCollection = message.Descriptor.Fields.InFieldNumberOrder();
            for (int tempIndex = 0; tempIndex < fieldCollection.Count; tempIndex++)
            {
                if (fieldCollection[tempIndex].FieldType == FieldType.Message)
                {
                    var temp = new ProtoParsedMessage
                    {
                        Id = tempIndex,
                        Name = fieldCollection[tempIndex].Name,
                        IsRepeated = fieldCollection[tempIndex].IsRepeated
                    };

                    protoParserMessage.Messages.Add(temp);

                    IMessage cleanSubmessage = fieldCollection[tempIndex].MessageType.Parser.ParseFrom(ByteString.Empty);
                    WriteMessageFields(temp, cleanSubmessage);
                }
                else
                {
                    object fieldValue = null;

                    var typeAndDefaultValue = GetTypeAndDefaultValue(fieldCollection[tempIndex]);
                    var minAndMaxValue = GetMinMaxValue(fieldCollection[tempIndex]);

                    if (IsPrimitiveType(fieldCollection[tempIndex]))
                    {
                        fieldValue = typeAndDefaultValue.Item2;
                    }

                    var field = new Field
                    {
                        Id = tempIndex,
                        Name = fieldCollection[tempIndex].Name,
                        DataType = typeAndDefaultValue.Item1,
                        Min = minAndMaxValue.Item1,
                        Max = minAndMaxValue.Item2,
                        Value = fieldValue,
                        DefaultValue = typeAndDefaultValue.Item2
                    };

                    if (fieldCollection[tempIndex].FieldType == FieldType.Enum)
                    {
                        var firstValue = fieldCollection[tempIndex].EnumType.Values.First();
                        var lastValue = fieldCollection[tempIndex].EnumType.Values.Last();

                        field.Min = firstValue.Number;
                        field.Max = lastValue.Number;
                    }

                    protoParserMessage.Fields.Add(field);
                }
            }
        }

        private bool IsPrimitiveType(FieldDescriptor descriptor)
        {
            switch (descriptor.FieldType)
            {
                case FieldType.Bool:
                case FieldType.Bytes:
                case FieldType.String:
                case FieldType.Double:
                case FieldType.SInt32:
                case FieldType.Int32:
                case FieldType.SFixed32:
                case FieldType.Enum:
                case FieldType.Fixed32:
                case FieldType.UInt32:
                case FieldType.Fixed64:
                case FieldType.UInt64:
                case FieldType.SFixed64:
                case FieldType.Int64:
                case FieldType.SInt64:
                case FieldType.Float:
                    return true;
                default:
                    return false;
            }
        }

        private Tuple<string, object> GetTypeAndDefaultValue(FieldDescriptor descriptor)
        {
            switch (descriptor.FieldType)
            {
                case FieldType.Bool:
                    return new Tuple<string, object>("bool", false);
                case FieldType.Bytes:
                case FieldType.String:
                    return new Tuple<string, object>("string", string.Empty);
                case FieldType.Double:
                    return new Tuple<string, object>("double", 0.0);
                case FieldType.SInt32:
                case FieldType.Int32:
                case FieldType.SFixed32:
                case FieldType.Fixed32:
                case FieldType.UInt32:
                case FieldType.Fixed64:
                case FieldType.UInt64:
                case FieldType.SFixed64:
                case FieldType.Int64:
                case FieldType.SInt64:
                    return new Tuple<string, object>("integer", 0);
                case FieldType.Enum:
                    return new Tuple<string, object>("enum", 0);
                case FieldType.Float:
                    return new Tuple<string, object>("float", 0.0);
                default:
                    throw new ArgumentException("Invalid field type");
            }
        }

        private Tuple<object, object> GetMinMaxValue(FieldDescriptor descriptor)
        {
            switch (descriptor.FieldType)
            {
                case FieldType.Bool:
                    return new Tuple<object, object>(0, 0);
                case FieldType.Bytes:
                case FieldType.String:
                    return new Tuple<object, object>("", string.Empty);
                case FieldType.Double:
                    return new Tuple<object, object>(double.MinValue, double.MaxValue);
                case FieldType.SInt32:
                    return new Tuple<object, object>(int.MinValue, int.MaxValue);
                case FieldType.Int32:
                    return new Tuple<object, object>(int.MinValue, int.MaxValue);
                case FieldType.SFixed32:
                    return new Tuple<object, object>(int.MinValue, int.MaxValue);
                case FieldType.Fixed32:
                    return new Tuple<object, object>(uint.MinValue, uint.MaxValue);
                case FieldType.UInt32:
                    return new Tuple<object, object>(uint.MinValue, uint.MaxValue);
                case FieldType.Fixed64:
                    return new Tuple<object, object>(ulong.MinValue, ulong.MaxValue);
                case FieldType.UInt64:
                    return new Tuple<object, object>(ulong.MinValue, ulong.MaxValue);
                case FieldType.SFixed64:
                case FieldType.Int64:
                case FieldType.SInt64:
                    return new Tuple<object, object>(long.MinValue, long.MaxValue);
                case FieldType.Enum:
                    return new Tuple<object, object>(0, 0);
                case FieldType.Float:
                    return new Tuple<object, object>(float.MinValue, float.MaxValue);
                default:
                    throw new ArgumentException("Invalid field type");
            }
        }
    }
}

