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

    public class ModuleParser
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

        public List<IMessage> GetAllMessages()
        {
            var messages = new List<IMessage>();
            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.GetInterfaces().Contains(typeof(IMessage))
                                     && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as IMessage;

            foreach (var instance in instances)
            {
                if (instance.Descriptor.Name == "Config" && CanConvertToMessageType(instance.GetType()))
                {
                    messages.Add(instance);
                }
            }

            return messages;
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
                        DataType = fieldDescriptor.FieldType.ToString().ToLower()
                    };

                    object fieldValue = null;

                    if (IsPrimitiveType(fieldDescriptor))
                    {
                        fieldValue = GetDefaultValue(fieldDescriptor);
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

        // <summary>
        // Called by method to ask if this object can serialize
        // an object of a given type.
        // </summary>
        // <returns>True if the objectType is a Protocol Message.</returns>
        private bool CanConvertToMessageType(Type objectType)
        {
            return typeof(IMessage)
                .IsAssignableFrom(objectType);
        }

        private object GetDefaultValue(FieldDescriptor descriptor)
        {
            switch (descriptor.FieldType)
            {
                case FieldType.Bool:
                    return false;
                case FieldType.Bytes:
                case FieldType.String:
                    return "\"\"";
                case FieldType.Double:
                    return 0.0;
                case FieldType.SInt32:
                case FieldType.Int32:
                case FieldType.SFixed32:
                case FieldType.Enum:
                    return (int)0;
                case FieldType.Fixed32:
                case FieldType.UInt32:
                    return (uint)0;
                case FieldType.Fixed64:
                case FieldType.UInt64:
                    return (ulong)0;
                case FieldType.SFixed64:
                case FieldType.Int64:
                case FieldType.SInt64:
                    return (long)0;
                case FieldType.Float:
                    return (float)0f;
                default:
                    throw new ArgumentException("Invalid field type");
            }
        }
    }
}
