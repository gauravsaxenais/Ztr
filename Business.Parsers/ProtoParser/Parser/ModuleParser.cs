namespace Business.Parsers.ProtoParser.Parser
{
    using EnsureThat;
    using Models;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ModuleParser : IModuleParser
    {
        public List<JsonField> MergeTomlWithProtoMessage<T>(T configValues, ProtoParsedMessage protoParsedMessage) where T : Dictionary<string, object>
        {
            var listOfData = new List<JsonField>();

            // add fields, repeated and non-repeated messages to the list
            // in case there is no data, just return fields, repeated and non-repeated messages
            // as basic data.
            listOfData.AddRange(AddEmptyFieldsAndArrays(protoParsedMessage));

            if (configValues.Any())
            {
                // remove repeated and non-repeated messages from the list
                // and keep fields only.
                RemoveEmptyArrays(listOfData);
            }

            foreach (var dicItem in configValues.Select((kvp, index) => new { Kvp = kvp, Index = index }))
            {
                var field = listOfData.FirstOrDefault(x =>
                     string.Equals(x.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (field != null)
                {
                    if (!field.IsRepeated)
                    {
                        field.Value = GetFieldValue(dicItem.Kvp.Value);
                        field.IsVisible = true;
                    }

                    else
                    {
                        var listOfFields = GetFieldsFromValue(field, dicItem.Kvp.Value);
                        listOfData.RemoveAll(item => item.Name == field.Name);
                        listOfData.AddRange(listOfFields);
                    }
                }

                // we have a repeated / non repeated protoParsedMessage.
                else
                {
                    foreach (var message in protoParsedMessage.Messages)
                    {
                        if (string.Equals(message.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            var tempJsonModel = new JsonField
                            {
                                Name = message.Name,
                                IsVisible = true,
                                DataType = "array"
                            };

                            if (dicItem.Kvp.Value is T[] repeatedValues)
                            {
                                repeatedValues.ToList().ForEach(item =>
                                    tempJsonModel.Arrays.Add(MergeTomlWithProtoMessage(item, message)));
                            }

                            else if (dicItem.Kvp.Value is T nonRepeatedValues)
                            {
                                var subArrayData = MergeTomlWithProtoMessage(nonRepeatedValues, message);
                                tempJsonModel.Fields.AddRange(subArrayData);
                            }

                            else
                            {
                                if (message.IsRepeated)
                                {
                                    tempJsonModel.Arrays.Add(MergeTomlWithProtoMessage(new Dictionary<string, object>(), message));
                                }

                                else
                                {
                                    tempJsonModel.Fields.AddRange(MergeTomlWithProtoMessage(new Dictionary<string, object>(), message));
                                }
                            }

                            listOfData.Add(tempJsonModel);
                            break;
                        }
                    }
                }
            }

            // fix the indexes
            FixIndex((listOfData));

            return listOfData;
        }

        #region Private methods
        private List<JsonField> AddEmptyFieldsAndArrays(ProtoParsedMessage protoParsedMessage)
        {
            var listOfData = new List<JsonField>();
            listOfData.AddRange(GetFieldsFromProtoMessage(protoParsedMessage));

            foreach (var message in protoParsedMessage.Messages)
            {
                var tempJsonModel = new JsonField
                {
                    Name = message.Name,
                    IsVisible = true,
                    DataType = "array"
                };

                if (message.IsRepeated)
                {
                    tempJsonModel.Arrays.Add(AddEmptyFieldsAndArrays(message));
                }

                else tempJsonModel.Fields.AddRange(AddEmptyFieldsAndArrays(message));

                listOfData.Add(tempJsonModel);
            }

            // fix the indexes
            FixIndex(listOfData);

            return listOfData;
        }

        private IEnumerable<JsonField> GetFieldsFromProtoMessage(ProtoParsedMessage protoParsedMessage)
        {
            EnsureArg.IsNotNull(protoParsedMessage);

            return protoParsedMessage.Fields.Select(field => (Field)field.Clone())
                .Select(newField => new JsonField
                {
                    Name = newField.Name,
                    Value = newField.Value,
                    Min = newField.Min,
                    Max = newField.Max,
                    DataType = newField.DataType,
                    DefaultValue = newField.DefaultValue,
                    IsVisible = false,
                    IsRepeated = newField.IsRepeated
                })
                .ToList();
        }
        #endregion

        #region Private helper methods

        private void FixIndex(IReadOnlyList<JsonField> listOfData)
        {
            for (var index = 0; index < listOfData.Count(); index++)
            {
                listOfData[index].Id = index;
            }
        }

        private void RemoveEmptyArrays(List<JsonField> listOfData)
        {
            if (listOfData != null && listOfData.Any())
            {
                var arrayTypes = listOfData.Where(x => x.DataType == "array").ToList();

                for (int index = 0; index < arrayTypes.Count(); index++)
                {
                    var fields = arrayTypes[index].Fields;
                    var arrays = arrayTypes[index].Arrays;

                    RemoveEmptyArrays(fields);
                    arrays.ForEach(RemoveEmptyArrays);

                    fields.Clear();
                    arrays.Clear();

                    listOfData.RemoveAll(t => t.Name == arrayTypes[index].Name);
                }
            }

            // fix the indexes
            FixIndex(listOfData);
        }
        private object GetFieldValue(object field)
        {
            object result;

            var fieldType = field.GetType();

            if (fieldType.IsArray)
            {
                var arrayResult = "[";

                var stringFields = ((IEnumerable)field).Cast<object>()
                                                                    .Select(x => x.ToString())
                                                                    .ToArray();
                arrayResult += string.Join(",", stringFields);
                arrayResult += "]";

                result = arrayResult;
            }

            else
            {
                result = field;
            }

            return result;
        }

        private List<JsonField> GetFieldsFromValue(JsonField jsonField, object field)
        {
            var fields = ((IEnumerable)field).Cast<object>().Select(x => x).ToList(); 

            var listofFields = new List<JsonField>();
            var fieldType = field.GetType();

            // just a sanity check to ensure field is an array.
            if (fieldType.IsArray)
            {
                if(!fields.Any())
                {
                    listofFields.Add(jsonField);
                }

                foreach (var tempItem in fields)
                {
                    var newField = new JsonField()
                    {
                        Name = jsonField.Name,
                        Value = tempItem,
                        Min = jsonField.Min,
                        Max = jsonField.Max,
                        DataType = jsonField.DataType,
                        DefaultValue = jsonField.DefaultValue,
                        IsVisible = true,
                        IsRepeated = jsonField.IsRepeated
                    };
                    listofFields.Add(newField);
                }
            }

            return listofFields;
            #endregion
        }
    }
}