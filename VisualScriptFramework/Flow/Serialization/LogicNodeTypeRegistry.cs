using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IoTLogic.Core.Reflection;

namespace IoTLogic.Flow.Serialization
{
    public sealed class LogicNodeTypeRegistry
    {
        private readonly Dictionary<string, Type> typesByKey;
        private readonly Dictionary<Type, string> keysByType;

        public static LogicNodeTypeRegistry Default { get; } = CreateDefault();

        public LogicNodeTypeRegistry()
        {
            typesByKey = new Dictionary<string, Type>(StringComparer.Ordinal);
            keysByType = new Dictionary<Type, string>();
        }

        public void Register(Type nodeType, string key)
        {
            if (nodeType == null)
            {
                throw new ArgumentNullException(nameof(nodeType));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A node type key is required.", nameof(key));
            }

            if (!typeof(LogicNode).IsAssignableFrom(nodeType))
            {
                throw new ArgumentException($"Type '{nodeType.FullName}' is not a logic node.", nameof(nodeType));
            }

            if (typesByKey.ContainsKey(key))
            {
                throw new InvalidOperationException($"Duplicate logic node key '{key}'.");
            }

            if (keysByType.ContainsKey(nodeType))
            {
                throw new InvalidOperationException($"Logic node type '{nodeType.FullName}' is already registered.");
            }

            typesByKey.Add(key, nodeType);
            keysByType.Add(nodeType, key);
        }

        public Type GetNodeType(string key)
        {
            if (!typesByKey.TryGetValue(key, out var nodeType))
            {
                throw new InvalidOperationException($"Unknown node type '{key}'.");
            }

            return nodeType;
        }

        public string GetNodeTypeKey(Type nodeType)
        {
            if (!keysByType.TryGetValue(nodeType, out var key))
            {
                throw new InvalidOperationException($"Unregistered logic node type '{nodeType.FullName}'.");
            }

            return key;
        }

        private static LogicNodeTypeRegistry CreateDefault()
        {
            var registry = new LogicNodeTypeRegistry();
            var assembly = typeof(LogicNode).Assembly;

            foreach (var nodeType in assembly.GetTypesSafely()
                .Where(type => typeof(LogicNode).IsAssignableFrom(type) && type.IsConcrete()))
            {
                registry.Register(nodeType, BuildKey(nodeType));
            }

            return registry;
        }

        private static string BuildKey(Type nodeType)
        {
            if (nodeType == typeof(SubgraphLogicNode))
            {
                return "nodes/nesting/subgraph";
            }

            var namespaceValue = nodeType.Namespace ?? string.Empty;

            if (string.Equals(namespaceValue, "IoTLogic.Flow.Nodes", StringComparison.Ordinal))
            {
                return "nodes/" + ToKebabCase(TrimSuffix(nodeType.Name, "Node"));
            }

            if (namespaceValue.StartsWith("IoTLogic.Flow.Nodes.", StringComparison.Ordinal))
            {
                return "nodes/" +
                    NormalizeNamespace(namespaceValue.Substring("IoTLogic.Flow.Nodes.".Length)) +
                    "/" +
                    ToKebabCase(TrimSuffix(nodeType.Name, "Node"));
            }

            if (string.Equals(namespaceValue, "IoTLogic.Flow.Framework", StringComparison.Ordinal))
            {
                return "framework/" + ToKebabCase(nodeType.Name);
            }

            if (namespaceValue.StartsWith("IoTLogic.Flow.Framework.", StringComparison.Ordinal))
            {
                return "framework/" +
                    NormalizeNamespace(namespaceValue.Substring("IoTLogic.Flow.Framework.".Length)) +
                    "/" +
                    ToKebabCase(nodeType.Name);
            }

            throw new InvalidOperationException($"Unable to derive a stable node key for '{nodeType.FullName}'.");
        }

        private static string NormalizeNamespace(string namespaceValue)
        {
            var parts = namespaceValue.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("/", parts.Select(ToKebabCase));
        }

        private static string TrimSuffix(string value, string suffix)
        {
            if (value.EndsWith(suffix, StringComparison.Ordinal))
            {
                return value.Substring(0, value.Length - suffix.Length);
            }

            return value;
        }

        private static string ToKebabCase(string value)
        {
            var builder = new StringBuilder();

            for (var index = 0; index < value.Length; index++)
            {
                var current = value[index];
                var isUpper = char.IsUpper(current);

                if (index > 0 && isUpper)
                {
                    var previous = value[index - 1];
                    var nextIsLower = index + 1 < value.Length && char.IsLower(value[index + 1]);

                    if (char.IsLower(previous) || char.IsDigit(previous) || nextIsLower)
                    {
                        builder.Append('-');
                    }
                }

                builder.Append(char.ToLowerInvariant(current));
            }

            return builder.ToString();
        }
    }
}
