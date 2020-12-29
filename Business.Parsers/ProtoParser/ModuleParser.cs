namespace Business.Parsers.ProtoParser
{
    using Business.Parsers.ProtoParser.Models;
    using EnsureThat;
    using Nett;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ZTR.Framework.Business.File.FileReaders;

    public class ModuleParser
    {
        public List<Dictionary<string, object>> GetListOfModulesFromTomlFile(string fileContent, TomlSettings settings)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(fileContent, (nameof(fileContent)));

            var fileData = Toml.ReadString(fileContent, settings);

            var dictionary = fileData.ToDictionary();
            var modules = (Dictionary<string, object>[])dictionary["module"];

            return modules.ToList();
        }

        public List<JsonField> GetJsonFromTomlAndProtoFile(string fileContent, ProtoParsedMessage protoParserMessage)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();
            var jsonModels = new List<JsonField>();
            var listOfModules = GetListOfModulesFromTomlFile(fileContent, tomlSettings);

            // here message.name means Power, j1939 etc.
            var module = listOfModules.Where(dic => dic.Values.Contains(protoParserMessage.Name.ToLower())).FirstOrDefault();

            if (module != null)
            {
                var configValues = new Dictionary<string, object>();

                if (module.ContainsKey("config"))
                {
                    configValues = (Dictionary<string, object>)module["config"];
                }

                jsonModels = MergeTomlWithProtoMessage(configValues, protoParserMessage);
            }

            return jsonModels;
        }

        public List<JsonField> MergeTomlWithProtoMessage(Dictionary<string, object> configValues, ProtoParsedMessage protoParsedMessage)
        {
            var listOfData = new List<JsonField>();
            listOfData.AddRange(AddFieldsToJsonModel(protoParsedMessage));

            foreach (var tempItem in configValues.Select((KeyValue, Index) => new { KeyValue, Index }))
            {
                var messages = protoParsedMessage.Messages;
                var fields = protoParsedMessage.Fields;

                var field = listOfData.Where(x => string.Equals(x.Name, tempItem.KeyValue.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var repeatedMessage = messages.Where(x => string.Equals(x.Name, tempItem.KeyValue.Key, StringComparison.OrdinalIgnoreCase) && x.IsRepeated).FirstOrDefault();

                // its a field
                if (field != null)
                {
                    field.Value = GetFieldValue(tempItem.KeyValue.Value);
                }

                else if (repeatedMessage != null)
                {
                    var jsonModel = new JsonField()
                    {
                        Name = repeatedMessage.Name
                    };

                    jsonModel.Arrays.AddRange(ProcessRepeatedMessage(repeatedMessage, tempItem.KeyValue.Value));
                    listOfData.Add(jsonModel);
                }
            }

            // fix the indexes.
            listOfData.Select((item, index) => { item.Id = index; return item; }).ToList();
            return listOfData;
        }

        private List<List<JsonField>> ProcessRepeatedMessage(ProtoParsedMessage repeatedMessage, object value)
        {
            var arrayData = new List<List<JsonField>>();
            var values = new List<Dictionary<string, object>>();

            if (value is Dictionary<string, object>[] v)
            {
                values.AddRange(v);
            }

            for (int temp = 0; temp < values.Count(); temp++)
            {
                var listOfData = new List<JsonField>();

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

                        var tempJsonModel = new JsonField
                        {
                            Id = dicItem.Index,
                        };

                        if (repeatedSubMessage != null)
                        {
                            tempJsonModel.Name = repeatedSubMessage.Name;
                            if (dicItem.Kvp.Value is Dictionary<string, object>[] repeatedValues)
                            {
                                var subArrayData = ProcessRepeatedMessage(repeatedSubMessage, repeatedValues);
                                tempJsonModel.Arrays.AddRange(subArrayData);
                            }
                        }

                        else if (nonRepeatedSubMessage != null)
                        {
                            tempJsonModel.Name = nonRepeatedSubMessage.Name;

                            if (dicItem.Kvp.Value is Dictionary<string, object> nonRepeatedValues)
                            {
                                var subArrayData = ProcessRepeatedMessage(nonRepeatedSubMessage, new Dictionary<string, object>[] { nonRepeatedValues });

                                var fields = subArrayData.FirstOrDefault();
                                tempJsonModel.Fields.AddRange(fields);
                            }
                        }

                        listOfData.Add(tempJsonModel);
                    }
                }

                arrayData.Add(listOfData);
            }

            return arrayData;
        }

        private List<JsonField> AddFieldsToJsonModel(ProtoParsedMessage protoParsedMessage)
        {
            EnsureArg.IsNotNull(protoParsedMessage);

            var listOfFields = new List<JsonField>();
            for (int tempIndex = 0; tempIndex < protoParsedMessage.Fields.Count; tempIndex++)
            {
                var newField = (Field)protoParsedMessage.Fields[tempIndex].Clone();

                var jsonModel = new JsonField
                {
                    Name = newField.Name,
                    Value = newField.Value,
                    Min = newField.Min,
                    Max = newField.Max,
                    DataType = newField.DataType,
                    DefaultValue = newField.DefaultValue
                };

                listOfFields.Add(jsonModel);
            }

            return listOfFields;
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

        private JsonField GetFieldsData(ProtoParsedMessage message, KeyValuePair<string, object> values)
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
                    var tempField = new JsonField
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
