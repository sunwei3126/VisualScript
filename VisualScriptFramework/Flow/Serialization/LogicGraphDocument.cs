using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTLogic.Flow.Serialization
{
    public sealed class LogicGraphDocument
    {
        public const int CurrentSchemaVersion = 1;

        public LogicGraphDocument()
        {
            SchemaVersion = CurrentSchemaVersion;
            Variables = new List<LogicGraphVariableDocument>();
            Nodes = new List<LogicGraphNodeDocument>();
            Edges = new List<LogicGraphEdgeDocument>();
        }

        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
        public string Summary { get; set; }

        [JsonProperty("variables")]
        public List<LogicGraphVariableDocument> Variables { get; private set; }

        [JsonProperty("nodes")]
        public List<LogicGraphNodeDocument> Nodes { get; private set; }

        [JsonProperty("edges")]
        public List<LogicGraphEdgeDocument> Edges { get; private set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Metadata { get; set; }
    }

    public sealed class LogicGraphVariableDocument
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public JToken Value { get; set; }
    }

    public sealed class LogicGraphNodeDocument
    {
        public LogicGraphNodeDocument()
        {
            Defaults = new Dictionary<string, JToken>(StringComparer.Ordinal);
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonProperty("defaults")]
        public Dictionary<string, JToken> Defaults { get; private set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Metadata { get; set; }
    }

    public sealed class LogicGraphEdgeDocument
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("fromNode")]
        public string FromNode { get; set; }

        [JsonProperty("fromPort")]
        public string FromPort { get; set; }

        [JsonProperty("toNode")]
        public string ToNode { get; set; }

        [JsonProperty("toPort")]
        public string ToPort { get; set; }
    }
}
