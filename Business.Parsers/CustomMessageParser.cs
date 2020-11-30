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
        public Message Format(IMessage message)
        {
            EnsureArg.IsNotNull(message);

            var messageName = Path.GetFileNameWithoutExtension(message.Descriptor.File.Name);
            var protoParserMessage = new Message() { Name = messageName };

            Format(message, protoParserMessage);

            return protoParserMessage;
        }

        /// <summary>
        /// Formats the specified message as Protoparser message.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <param name="protoParserMessage">The field to parse the formatted message to.</param>
        /// <returns>The formatted message.</returns>
        public void Format(IMessage message, Message protoParserMessage)
        {
            ProtoPreconditions.CheckNotNull(message, nameof(message));

            WriteMessage(protoParserMessage, message);
        }

        private void WriteMessage(Message protoParserMessage, IMessage message)
        {
            if (message == null)
            {
                return;
            }

            WriteMessageFields(protoParserMessage, message);
        }

        private void WriteMessageFields(Message protoParserMessage, IMessage message)
        {
            foreach (var fieldDescriptor in message.Descriptor.Fields.InFieldNumberOrder())
            {
                if (fieldDescriptor.FieldType == FieldType.Message)
                {
                    var temp = new Message
                    {
                        Name = fieldDescriptor.Name,
                        IsRepeated = fieldDescriptor.IsRepeated
                    };

                    protoParserMessage.Messages.Add(temp);

                    IMessage cleanSubmessage = fieldDescriptor.MessageType.Parser.ParseFrom(ByteString.Empty);
                    WriteMessageFields(temp, cleanSubmessage);
                }
                else
                {
                    var field = new Field
                    {
                        Name = fieldDescriptor.Name,
                        DataType = GetTypeAndDefaultValue(fieldDescriptor).Item1
                    };

                    object fieldValue = null;

                    if (IsPrimitiveType(fieldDescriptor))
                    {
                        fieldValue = GetTypeAndDefaultValue(fieldDescriptor).Item2;
                    }

                    var minValue = fieldValue;
                    var maxValue = fieldValue;

                    if (fieldDescriptor.FieldType == FieldType.Enum)
                    {
                        var firstValue = fieldDescriptor.EnumType.Values.First();
                        var lastValue = fieldDescriptor.EnumType.Values.Last();

                        minValue = firstValue.Number.ToString();
                        maxValue = lastValue.Number.ToString();
                    }

                    field.Min = minValue;
                    field.Max = maxValue;
                    field.Value = fieldValue;

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
