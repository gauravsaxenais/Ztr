namespace Business.Parsers
{
    using Business.Parser.Models;
    using EnsureThat;
    using Google.Protobuf;
    using Google.Protobuf.Reflection;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class CustomMessageParser
    {
        /// <summary>
        /// Formats the specified message as JSON.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <returns>The formatted message.</returns>
        public Parser.Models.Message Format(IMessage message)
        {
            EnsureArg.IsNotNull(message);

            var messageName = Path.GetFileNameWithoutExtension(message.Descriptor.File.Name);
            var protoParserMessage = new Parser.Models.Message() { Name = messageName };

            Format(message, protoParserMessage);

            return protoParserMessage;
        }

        /// <summary>
        /// Formats the specified message as Protoparser message.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <param name="protoParserMessage">The field to parse the formatted message to.</param>
        /// <returns>The formatted message.</returns>
        public void Format(IMessage message, Parser.Models.Message protoParserMessage)
        {
            ProtoPreconditions.CheckNotNull(message, nameof(message));

            WriteMessage(protoParserMessage, message);
        }

        private void WriteMessage(Parser.Models.Message protoParserMessage, IMessage message)
        {
            if (message == null)
            {
                return;
            }

            WriteMessageFields(protoParserMessage, message);
        }

        private void WriteMessageFields(Parser.Models.Message protoParserMessage, IMessage message)
        {
            var fieldCollection = message.Descriptor.Fields.InFieldNumberOrder();
            for (int tempIndex = 0; tempIndex < fieldCollection.Count; tempIndex++)
            {
                if (fieldCollection[tempIndex].FieldType == FieldType.Message)
                {
                    var temp = new Parser.Models.Message
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

                    if (IsPrimitiveType(fieldCollection[tempIndex]))
                    {
                        fieldValue = GetTypeAndDefaultValue(fieldCollection[tempIndex]).Item2;
                    }

                    var field = new Field
                    {
                        Id = tempIndex,
                        Name = fieldCollection[tempIndex].Name,
                        DataType = GetTypeAndDefaultValue(fieldCollection[tempIndex]).Item1,
                        Min = fieldValue,
                        Max = fieldValue,
                        Value = fieldValue
                    };

                    if (fieldCollection[tempIndex].FieldType == FieldType.Enum)
                    {
                        var firstValue = fieldCollection[tempIndex].EnumType.Values.First();
                        var lastValue = fieldCollection[tempIndex].EnumType.Values.Last();

                       field.Min = firstValue.Number;
                       field.Max = lastValue.Number;
                    }

                    var fields = protoParserMessage.Fields.FirstOrDefault();
                    if (fields == null)
                    {
                        fields = new List<Field>();
                    }

                    fields.Add(field);

                    protoParserMessage.Fields.Add(fields);
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
                    return new Tuple <string, object>("bool", false);
                case FieldType.Bytes:
                case FieldType.String:
                    return new Tuple<string, object>("string", "\"\"");
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
    }
}
