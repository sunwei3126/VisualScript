using System;
using System.Collections.Generic;
using System.Linq;
using IoTLogic.Core.Variables;
using IoTLogic.Flow;
using IoTLogic.Flow.Connections;
using Newtonsoft.Json.Linq;

namespace IoTLogic.Flow.Serialization
{
    public sealed class LogicGraphDocumentCompiler
    {
        public LogicGraphDocumentCompiler() : this(LogicNodeTypeRegistry.Default) { }

        public LogicGraphDocumentCompiler(LogicNodeTypeRegistry typeRegistry)
        {
            TypeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
        }

        public LogicNodeTypeRegistry TypeRegistry { get; }

        public static LogicGraph Compile(LogicGraphDocument document)
        {
            return new LogicGraphDocumentCompiler().CompileDocument(document);
        }

        public static LogicGraphDocument Export(LogicGraph graph)
        {
            return new LogicGraphDocumentCompiler().ExportGraph(graph);
        }

        public LogicGraph CompileDocument(LogicGraphDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.SchemaVersion != LogicGraphDocument.CurrentSchemaVersion)
            {
                throw new InvalidOperationException(
                    $"Unsupported schema version '{document.SchemaVersion}'. Expected '{LogicGraphDocument.CurrentSchemaVersion}'.");
            }

            var graph = new LogicGraph
            {
                Title = document.Title,
                Summary = document.Summary,
            };

            var nodesById = new Dictionary<string, ILogicNode>(StringComparer.Ordinal);

            foreach (var variable in document.Variables ?? Enumerable.Empty<LogicGraphVariableDocument>())
            {
                if (variable == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(variable.Name))
                {
                    throw new InvalidOperationException("Graph variable documents must define a name.");
                }

                graph.Variables.Set(
                    variable.Name,
                    LogicGraphValueCodec.Decode(variable.Value, typeof(object), $"graph variable '{variable.Name}'"));
            }

            foreach (var nodeDocument in document.Nodes ?? Enumerable.Empty<LogicGraphNodeDocument>())
            {
                if (nodeDocument == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(nodeDocument.Id))
                {
                    throw new InvalidOperationException("Every node document must define an id.");
                }

                if (nodesById.ContainsKey(nodeDocument.Id))
                {
                    throw new InvalidOperationException($"Found duplicate node id '{nodeDocument.Id}'.");
                }

                var nodeType = TypeRegistry.GetNodeType(nodeDocument.Type);

                if (typeof(INesterUnit).IsAssignableFrom(nodeType) || typeof(SubgraphLogicNode).IsAssignableFrom(nodeType))
                {
                    throw new InvalidOperationException($"Subgraph node type '{nodeDocument.Type}' is not supported by JSON schema v1.");
                }

                if (!Guid.TryParse(nodeDocument.Id, out var nodeId))
                {
                    throw new InvalidOperationException($"Node id '{nodeDocument.Id}' is not a valid GUID.");
                }

                var node = (ILogicNode)Activator.CreateInstance(nodeType, nonPublic: true);
                ((LogicNode)node).Guid = nodeId;
                graph.LogicNodes.Add(node);
                node.EnsureDefined();
                nodesById.Add(nodeDocument.Id, node);
            }

            foreach (var nodeDocument in document.Nodes ?? Enumerable.Empty<LogicGraphNodeDocument>())
            {
                if (nodeDocument == null)
                {
                    continue;
                }

                var node = nodesById[nodeDocument.Id];

                foreach (var defaultValue in nodeDocument.Defaults ?? Enumerable.Empty<KeyValuePair<string, JToken>>())
                {
                    if (!node.ValueInputs.Contains(defaultValue.Key))
                    {
                        throw new InvalidOperationException(
                            $"Node '{nodeDocument.Id}' has an unknown port '{defaultValue.Key}' in defaults.");
                    }

                    var port = node.ValueInputs[defaultValue.Key];
                    node.DefaultValues[defaultValue.Key] = LogicGraphValueCodec.Decode(
                        defaultValue.Value,
                        port.Type,
                        $"default '{defaultValue.Key}' on node '{nodeDocument.Id}'");
                }
            }

            foreach (var edge in document.Edges ?? Enumerable.Empty<LogicGraphEdgeDocument>())
            {
                if (edge == null)
                {
                    continue;
                }

                if (string.Equals(edge.Kind, "control", StringComparison.Ordinal))
                {
                    CompileControlEdge(nodesById, graph, edge);
                    continue;
                }

                if (string.Equals(edge.Kind, "value", StringComparison.Ordinal))
                {
                    CompileValueEdge(nodesById, graph, edge);
                    continue;
                }

                throw new InvalidOperationException($"Unsupported edge kind '{edge.Kind}'.");
            }

            if (graph.InvalidConnections.Any())
            {
                throw new InvalidOperationException("Compiled graph contains invalid connections, which are not supported by JSON schema v1.");
            }

            return graph;
        }

        public LogicGraphDocument ExportGraph(LogicGraph graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (graph.InvalidConnections.Any())
            {
                throw new InvalidOperationException("Graphs with invalid connections cannot be exported to JSON schema v1.");
            }

            var document = new LogicGraphDocument
            {
                Title = graph.Title,
                Summary = graph.Summary,
            };

            foreach (var variable in graph.Variables)
            {
                document.Variables.Add(new LogicGraphVariableDocument
                {
                    Name = variable.Name,
                    Value = LogicGraphValueCodec.Encode(variable.Value, $"graph variable '{variable.Name}'"),
                });
            }

            foreach (var node in graph.LogicNodes)
            {
                if (node is INesterUnit || node is SubgraphLogicNode)
                {
                    throw new InvalidOperationException($"JSON schema v1 does not support subgraph node '{node.GetType().Name}'.");
                }

                node.EnsureDefined();

                var nodeDocument = new LogicGraphNodeDocument
                {
                    Id = ((LogicNode)node).Guid.ToString("D"),
                    Type = TypeRegistry.GetNodeTypeKey(node.GetType()),
                };

                foreach (var defaultValue in node.DefaultValues.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    nodeDocument.Defaults.Add(
                        defaultValue.Key,
                        LogicGraphValueCodec.Encode(defaultValue.Value, $"default '{defaultValue.Key}' on node '{nodeDocument.Id}'"));
                }

                document.Nodes.Add(nodeDocument);
            }

            foreach (var connection in graph.ControlConnections)
            {
                document.Edges.Add(new LogicGraphEdgeDocument
                {
                    Kind = "control",
                    FromNode = ((LogicNode)connection.Source.LogicNode).Guid.ToString("D"),
                    FromPort = connection.Source.Key,
                    ToNode = ((LogicNode)connection.Destination.LogicNode).Guid.ToString("D"),
                    ToPort = connection.Destination.Key,
                });
            }

            foreach (var connection in graph.ValueConnections)
            {
                document.Edges.Add(new LogicGraphEdgeDocument
                {
                    Kind = "value",
                    FromNode = ((LogicNode)connection.Source.LogicNode).Guid.ToString("D"),
                    FromPort = connection.Source.Key,
                    ToNode = ((LogicNode)connection.Destination.LogicNode).Guid.ToString("D"),
                    ToPort = connection.Destination.Key,
                });
            }

            return document;
        }

        private static void CompileControlEdge(
            IDictionary<string, ILogicNode> nodesById,
            LogicGraph graph,
            LogicGraphEdgeDocument edge)
        {
            var sourceNode = GetNode(nodesById, edge.FromNode, "source");
            var destinationNode = GetNode(nodesById, edge.ToNode, "destination");

            if (!sourceNode.ControlOutputs.Contains(edge.FromPort))
            {
                throw new InvalidOperationException(
                    $"Edge from node '{edge.FromNode}' references unknown port '{edge.FromPort}'.");
            }

            if (!destinationNode.ControlInputs.Contains(edge.ToPort))
            {
                throw new InvalidOperationException(
                    $"Edge to node '{edge.ToNode}' references unknown port '{edge.ToPort}'.");
            }

            try
            {
                graph.ControlConnections.Add(new ControlConnection(
                    sourceNode.ControlOutputs[edge.FromPort],
                    destinationNode.ControlInputs[edge.ToPort]));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to create control edge '{edge.FromNode}.{edge.FromPort} -> {edge.ToNode}.{edge.ToPort}'.",
                    ex);
            }
        }

        private static void CompileValueEdge(
            IDictionary<string, ILogicNode> nodesById,
            LogicGraph graph,
            LogicGraphEdgeDocument edge)
        {
            var sourceNode = GetNode(nodesById, edge.FromNode, "source");
            var destinationNode = GetNode(nodesById, edge.ToNode, "destination");

            if (!sourceNode.ValueOutputs.Contains(edge.FromPort))
            {
                throw new InvalidOperationException(
                    $"Edge from node '{edge.FromNode}' references unknown port '{edge.FromPort}'.");
            }

            if (!destinationNode.ValueInputs.Contains(edge.ToPort))
            {
                throw new InvalidOperationException(
                    $"Edge to node '{edge.ToNode}' references unknown port '{edge.ToPort}'.");
            }

            try
            {
                graph.ValueConnections.Add(new ValueConnection(
                    sourceNode.ValueOutputs[edge.FromPort],
                    destinationNode.ValueInputs[edge.ToPort]));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to create value edge '{edge.FromNode}.{edge.FromPort} -> {edge.ToNode}.{edge.ToPort}'.",
                    ex);
            }
        }

        private static ILogicNode GetNode(IDictionary<string, ILogicNode> nodesById, string nodeId, string edgeEnd)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                throw new InvalidOperationException($"Edge is missing its {edgeEnd} node id.");
            }

            if (!nodesById.TryGetValue(nodeId, out var node))
            {
                throw new InvalidOperationException($"Edge references unknown {edgeEnd} node '{nodeId}'.");
            }

            return node;
        }
    }
}
