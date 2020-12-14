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

            var jsonModel = new JsonModel
            {
                Name = protoParserMessage.Name,
                Arrays = new List<object>()
            };

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
            for (int tempIndex = 0; tempIndex < configValues.Count; tempIndex++)
            {
                var key = configValues.ElementAt(tempIndex).Key;

                var messages = protoParsedMessage.Messages;
                var fields = protoParsedMessage.Fields;

                var field = fields.Where(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var repeatedMessage = messages.Where(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                // its a field
                if (field != null)
                {
                    field.Id = tempIndex;
                    field.Value = GetFieldValue(configValues.ElementAt(tempIndex).Value);

                    var newField = (Field)field.Clone();
                    model.Fields.Add(newField);
                }

                else if (repeatedMessage != null)
                {
                    var defaultValuesFromToml = new List<Dictionary<string, object>>();

                    var jsonArray = new JsonArray
                    {
                        Name = repeatedMessage.Name,
                        IsRepeated = repeatedMessage.IsRepeated
                    };

                    if (configValues.ElementAt(tempIndex).Value is Dictionary<string, object>[] v)
                    {
                        defaultValuesFromToml.AddRange(v);
                    }

                    ProcessArray(defaultValuesFromToml, jsonArray, repeatedMessage);

                    model.Arrays.Add(jsonArray);
                }
            }
        }

        private void ProcessArray(List<Dictionary<string, object>> defaultValuesFromToml, JsonArray jsonArray, ProtoParsedMessage repeatedMessage)
        {
            foreach (var defaultValueFromToml in defaultValuesFromToml)
            {
                var arrayOfFields = new List<object>();

                for (int temp = 0; temp < defaultValueFromToml.Count(); temp++)
                {
                    var messageField = repeatedMessage.Fields.Where(x => x.Name == defaultValueFromToml.ElementAt(temp).Key).FirstOrDefault();
                    var messageMessage = repeatedMessage.Messages.Where(x => x.Name == defaultValueFromToml.ElementAt(temp).Key).FirstOrDefault();

                    if (messageField != null)
                    {
                        var tempField = (Field)messageField.Clone();
                        tempField.Value = defaultValueFromToml.ElementAt(temp).Value;

                        // fix the indexes.
                        arrayOfFields.Add(tempField);
                    }

                    else if (messageMessage != null)
                    {
                        if (defaultValueFromToml.ElementAt(temp).Value is Dictionary<string, object>[] tomlValue)
                        {
                            var tempJsonModel = new JsonModel
                            {
                                Id = temp,
                                Name = messageMessage.Name
                            };

                            var tempJsonArray = new JsonArray
                            {
                                Name = messageMessage.Name,
                                IsRepeated = messageMessage.IsRepeated
                            };

                            ProcessArray(tomlValue.ToList(), tempJsonArray, messageMessage);

                            tempJsonModel.Arrays.Add(tempJsonArray);
                            arrayOfFields.Add(tempJsonModel);
                        }
                    }
                }

                jsonArray.Data.Add(arrayOfFields);
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

        private List<Field> GetFieldsData(ProtoParsedMessage message, Dictionary<string, object> values)
        {
            var arrayOfFields = new List<Field>();
            var fields = message.Fields;

            if (fields == null || !fields.Any() || values == null || !values.Any())
            {
                return arrayOfFields;
            }

            for (int tempIndex = 0; tempIndex < fields.Count; tempIndex++)
            {
                if (values.ContainsKey(fields[tempIndex].Name))
                {
                    var tempField = (Field)fields[tempIndex].Clone();
                    tempField.Value = values[fields[tempIndex].Name];

                    // fix the indexes.
                    arrayOfFields.Add(tempField);
                }
            }

            return arrayOfFields;
        }
    }
}
