using System;
using System.IO;
using Newtonsoft.Json;

namespace IoTLogic.Flow.Serialization
{
    public static class LogicGraphDocumentSerializer
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static LogicGraphDocument Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A JSON file path is required.", nameof(path));
            }

            return Deserialize(File.ReadAllText(path));
        }

        public static void Save(string path, LogicGraphDocument document)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A JSON file path is required.", nameof(path));
            }

            var directory = Path.GetDirectoryName(Path.GetFullPath(path));

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, Serialize(document));
        }

        public static LogicGraphDocument Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON content is required.", nameof(json));
            }

            var document = JsonConvert.DeserializeObject<LogicGraphDocument>(json, serializerSettings);

            if (document == null)
            {
                throw new InvalidOperationException("Unable to deserialize a logic graph document from JSON.");
            }

            return document;
        }

        public static string Serialize(LogicGraphDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonConvert.SerializeObject(document, serializerSettings);
        }
    }
}
