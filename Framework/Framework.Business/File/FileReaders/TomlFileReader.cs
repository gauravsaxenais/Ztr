namespace ZTR.Framework.Business.File.FileReaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using Nett;
    using Content;
    using Models;

    public static class TomlFileReader
    {
        public static T ReadDataFromString<T>(string data, TomlSettings settings) where T : class, new()
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(data, nameof(data));
            EnsureArg.IsNotNull(settings, nameof(settings));

            T fileData;
            try
            {
                fileData = Toml.ReadString<T>(data, settings);
            }
            catch (Exception exception)
            {
                throw new CustomArgumentException(Resource.TomlParsingError, exception);
            }

            return fileData;
        }

        public static object ReadDataFromFile(string filePath, string fieldToRead, TomlSettings settings)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(filePath, (nameof(filePath)));
            EnsureArg.IsNotNull(settings, nameof(settings));

            try
            {
                var data = Toml.ReadFile(filePath, settings);
                return data.Get<object>(fieldToRead);
            }
            catch (Exception exception)
            {
                throw new CustomArgumentException(Resource.TomlParsingError, exception);
            }
        }

        public static List<Dictionary<string, object>> ReadDataFromFile(string filePath, TomlSettings settings, string fieldToRead)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(filePath, (nameof(filePath)));
            EnsureArg.IsNotNull(settings, nameof(settings));
            EnsureArg.IsNotEmptyOrWhiteSpace(fieldToRead, nameof(fieldToRead));

            var items = new List<Dictionary<string, object>>();

            try
            {
                TomlTable fileData = null;

                fileData = Toml.ReadFile(filePath, settings);

                var readModels = (TomlTableArray)fileData[fieldToRead];

                foreach (var tempItem in readModels.Items)
                {
                    var dictionary = tempItem.Rows.ToDictionary(t => t.Key, t => (object)t.Value.ToString());

                    items.Add(dictionary);
                }
            }
            catch (Exception exception)
            {
                throw new CustomArgumentException(Resource.TomlParsingError, exception);
            }

            return items;
        }

        
        public static TomlSettings LoadLowerCaseTomlSettings()
        {
            var fieldNamesToLowerCaseSettings = TomlSettings.Create(config => config
            .ConfigurePropertyMapping(mapping => mapping
            .UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));

            return fieldNamesToLowerCaseSettings;
        }
    }
}
