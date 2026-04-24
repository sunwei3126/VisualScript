using System;
using System.Collections.Generic;
using IoTLogic.Flow.Connections;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow
{
    /// <summary>
    /// Preserves port connections of a LogicNode before redefinition and restores
    /// them afterwards if the corresponding ports still exist.
    /// </summary>
    public sealed class UnitPreservation
    {
        private struct ConnectionEntry
        {
            public ILogicNode SourceUnit;
            public string SourceKey;
            public ILogicNode DestinationUnit;
            public string DestinationKey;
            public ConnectionKind Kind;
        }

        private enum ConnectionKind { Control, Value }

        private readonly List<ConnectionEntry> _entries = new List<ConnectionEntry>();

        private UnitPreservation() { }

        public static UnitPreservation Preserve(ILogicNode node)
        {
            var p = new UnitPreservation();

            if (node.Graph == null)
                return p;

            foreach (var conn in node.Graph.ControlConnections)
            {
                if (conn.Source.LogicNode == node || conn.Destination.LogicNode == node)
                {
                    p._entries.Add(new ConnectionEntry
                    {
                        SourceUnit = conn.Source.LogicNode,
                        SourceKey = conn.Source.Key,
                        DestinationUnit = conn.Destination.LogicNode,
                        DestinationKey = conn.Destination.Key,
                        Kind = ConnectionKind.Control
                    });
                }
            }

            foreach (var conn in node.Graph.ValueConnections)
            {
                if (conn.Source.LogicNode == node || conn.Destination.LogicNode == node)
                {
                    p._entries.Add(new ConnectionEntry
                    {
                        SourceUnit = conn.Source.LogicNode,
                        SourceKey = conn.Source.Key,
                        DestinationUnit = conn.Destination.LogicNode,
                        DestinationKey = conn.Destination.Key,
                        Kind = ConnectionKind.Value
                    });
                }
            }

            return p;
        }

        public void RestoreTo(ILogicNode node)
        {
            if (node.Graph == null)
                return;

            foreach (var entry in _entries)
            {
                try
                {
                    if (entry.Kind == ConnectionKind.Control)
                    {
                        if (entry.SourceUnit.ControlOutputs.Contains(entry.SourceKey) &&
                            entry.DestinationUnit.ControlInputs.Contains(entry.DestinationKey))
                        {
                            var source = entry.SourceUnit.ControlOutputs[entry.SourceKey];
                            var dest = entry.DestinationUnit.ControlInputs[entry.DestinationKey];
                            if (!source.HasValidConnection && !dest.HasAnyConnection)
                            {
                                node.Graph.ControlConnections.Add(new ControlConnection(source, dest));
                            }
                        }
                    }
                    else
                    {
                        if (entry.SourceUnit.ValueOutputs.Contains(entry.SourceKey) &&
                            entry.DestinationUnit.ValueInputs.Contains(entry.DestinationKey))
                        {
                            var source = entry.SourceUnit.ValueOutputs[entry.SourceKey];
                            var dest = entry.DestinationUnit.ValueInputs[entry.DestinationKey];
                            if (!dest.HasAnyConnection)
                            {
                                node.Graph.ValueConnections.Add(new ValueConnection(source, dest));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restore connection {entry.SourceKey} -> {entry.DestinationKey}: {ex.Message}");
                }
            }
        }
    }
}