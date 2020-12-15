namespace Business.Parsers
{
    using Business.Parsers.Models;
    using EnsureThat;
    using Nett;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ModuleParser
    {
        public JsonModel GetJsonFromDefaultValueAndProtoFile(string fileContent, TomlSettings settings, ProtoParsedMessage protoParserMessage)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(fileContent, (nameof(fileContent)));

            var jsonModel = new JsonModel();

            var fileData = Toml.ReadString(fileContent, settings);

            var dictionary = fileData.ToDictionary();
            var module = (Dictionary<string, object>[])dictionary["module"];

            // here message.name means Power, j1939 etc.
            var moduleDetail = module.Where(dic => dic.Values.Contains(protoParserMessage.Name.ToLower())).FirstOrDefault();

            if (moduleDetail != null)
            {
                var configValues = new Dictionary<string, object>();

                if (moduleDetail.ContainsKey("config"))
                {
                    configValues = (Dictionary<string, object>)moduleDetail["config"];
                }

                MergeTomlWithProtoMessage(configValues, protoParserMessage, jsonModel);
            }

            return jsonModel;
        }

        public void MergeTomlWithProtoMessage(Dictionary<string, object> configValues, ProtoParsedMessage protoParsedMessage, JsonModel model)
        {
            model.Name = protoParsedMessage.Name;
            AddFieldsToJsonModel(protoParsedMessage, model);

            foreach (var tempItem in configValues.Select((KeyValue, Index) => new { KeyValue, Index }))
            {
                var messages = protoParsedMessage.Messages;
                var fields = protoParsedMessage.Fields;

                var field = model.Fields.Where(x => string.Equals(x.Name, tempItem.KeyValue.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var repeatedMessage = messages.Where(x => string.Equals(x.Name, tempItem.KeyValue.Key, StringComparison.OrdinalIgnoreCase) && x.IsRepeated).FirstOrDefault();
                
                // its a field
                if (field != null)
                {
                    field.Value = GetFieldValue(tempItem.KeyValue.Value);
                }

                else if (repeatedMessage != null)
                {
                    ProcessRepeatedMessage(model, repeatedMessage, tempItem.KeyValue.Value);
                }
            }
        }

        private void ProcessRepeatedMessage(JsonModel model, ProtoParsedMessage repeatedMessage, object value)
        {
            var values = new List<Dictionary<string, object>>();

            if (value is Dictionary<string, object>[] v)
            {
                values.AddRange(v);
            }

            for (int temp = 0; temp < values.Count(); temp++)
            {
                var listOfData = new List<JsonModel>();

                foreach (var dicItem in values[temp].Select((Kvp, Index) => new { Kvp, Index }))
                {
                    if (HasFieldData(repeatedMessage, dicItem.Kvp))
                    {
                        var fieldsWithData = GetFieldsData(repeatedMessage, dicItem.Kvp);

                        fieldsWithData.Id = dicItem.Index;
                        listOfData.Add(fieldsWithData);
                    }

                    // we have a repeated message like an array.
                    else
                    {
                        var repeatedSubMessage = repeatedMessage.Messages.Where(x => string.Equals(x.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase) && x.IsRepeated).FirstOrDefault();
                        var nonRepeatedSubMessage = repeatedMessage.Messages.Where(x => string.Equals(x.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase) && !x.IsRepeated).FirstOrDefault();

                        if (repeatedSubMessage != null)
                        {
                            if (dicItem.Kvp.Value is Dictionary<string, object>[] tempValue)
                            {
                                var tempJsonModel = new JsonModel
                                {
                                    Id = dicItem.Index,
                                    Name = repeatedSubMessage.Name
                                };

                                ProcessRepeatedMessage(tempJsonModel, repeatedSubMessage, tempValue);

                                listOfData.Add(tempJsonModel);
                            }
                        }

                        else if (nonRepeatedSubMessage != null)
                        {
                            if (dicItem.Kvp.Value is Dictionary<string, object> tempValue)
                            {
                                var tempJsonModel = new JsonModel
                                {
                                    Id = dicItem.Index,
                                    Name = nonRepeatedSubMessage.Name
                                };

                                foreach (var tempDict in tempValue.Select((TempKvp, TempIndex) => new { TempKvp, TempIndex }))
                                {
                                    if (HasFieldData(nonRepeatedSubMessage, tempDict.TempKvp))
                                    {
                                        var fieldsWithData = GetFieldsData(nonRepeatedSubMessage, tempDict.TempKvp);

                                        fieldsWithData.Id = dicItem.Index;
                                        tempJsonModel.Fields.Add(fieldsWithData);
                                    }
                                }

                                listOfData.Add(tempJsonModel);
                            }
                        }
                    }
                }

                model.Arrays.Add(listOfData);
            }
        }

        private void AddFieldsToJsonModel(ProtoParsedMessage protoParsedMessage, JsonModel model)
        {
            for (int tempIndex = 0; tempIndex < protoParsedMessage.Fields.Count; tempIndex++)
            {
                var newField = (Field)protoParsedMessage.Fields[tempIndex].Clone();

                var jsonModel = new JsonModel
                {
                    Id = tempIndex,
                    Name = newField.Name,
                    Value = newField.Value,
                    Min = newField.Min,
                    Max = newField.Max,
                    DefaultValue = newField.DefaultValue
                };

                model.Fields.Add(jsonModel);
            }
        }

        private bool IsValueType(object obj)
        {
            var objType = obj.GetType();
            return obj != null && objType.GetTypeInfo().IsValueType;
        }

        private object GetFieldValue(object field)
        {
            object result = new object();

            var stringType = typeof(string);
            var fieldType = field.GetType();

            if (fieldType.IsArray)
            {
                var arrayResult = "[";
                var element = ((IEnumerable)field).Cast<object>().FirstOrDefault();

                if (IsValueType(element))
                {
                    IEnumerable fields = field as IEnumerable;

                    foreach (var tempItem in fields)
                    {
                        arrayResult += tempItem + ",";
                    }

                    arrayResult += arrayResult.TrimEnd(',');
                }

                else if (stringType.IsAssignableFrom(element.GetType()))
                {
                    string[] stringFields = ((IEnumerable)field).Cast<object>()
                                                                .Select(x => x.ToString())
                                                                .ToArray();

                    arrayResult += string.Join(",", stringFields);
                }

                arrayResult += "]";

                result = arrayResult;
            }

            else
            {
                result = field;
            }

            return result;
        }

        private JsonModel GetFieldsData(ProtoParsedMessage message, KeyValuePair<string, object> values)
        {
            var fields = message.Fields;

            if (fields == null || !fields.Any() || values.Equals(default(KeyValuePair<string, object>)))
            {
                return null;
            }

            for (int tempIndex = 0; tempIndex < fields.Count; tempIndex++)
            {
                if (values.Key.Equals(fields[tempIndex].Name))
                {
                    var tempField = new JsonModel
                    {
                        Max = fields[tempIndex].Max,
                        Min = fields[tempIndex].Min,
                        Name = fields[tempIndex].Name,
                        DataType = fields[tempIndex].DataType,
                        DefaultValue = fields[tempIndex].DefaultValue,
                        Value = values.Value
                    };

                    return tempField;
                }
            }

            return null;
        }

        private bool HasFieldData(ProtoParsedMessage message, KeyValuePair<string, object> values)
        {
            var fields = message?.Fields;

            if (fields == null || !fields.Any() || values.Equals(default(KeyValuePair<string, object>)))
            {
                return false;
            }

            for (int tempIndex = 0; tempIndex < fields.Count; tempIndex++)
            {
                if (values.Key.Equals(fields[tempIndex].Name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
