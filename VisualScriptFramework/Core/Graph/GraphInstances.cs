using System;
using System.Collections.Generic;
using IoTLogic.Core.Pooling;
using IoTLogic.Core.Utities;

namespace IoTLogic.Core.Graph
{
    public static class GraphInstances
    {
        private static readonly object @lock = new object();

        private static readonly Dictionary<IGraph, HashSet<GraphReference>> byGraph = new Dictionary<IGraph, HashSet<GraphReference>>();

        private static readonly Dictionary<IGraphParent, HashSet<GraphReference>> byParent = new Dictionary<IGraphParent, HashSet<GraphReference>>();

        public static void Instantiate(GraphReference instance)
        {
            lock (@lock)
            {
                Ensure.Ensure.That(nameof(instance)).IsNotNull(instance);

                instance.CreateGraphData();

                instance.Graph.Instantiate(instance);

                if (!byGraph.TryGetValue(instance.Graph, out var instancesWithGraph))
                {
                    instancesWithGraph = new HashSet<GraphReference>();
                    byGraph.Add(instance.Graph, instancesWithGraph);
                }

                if (instancesWithGraph.Add(instance))
                {
                    // Debug.Log($"Added graph instance mapping:\n{instance.graph} => {instance}");
                }
                else
                {
                   Console.WriteLine($"Attempting to add duplicate graph instance mapping:\n{instance.Graph} => {instance}");
                }

                if (!byParent.TryGetValue(instance.Parent, out var instancesWithParent))
                {
                    instancesWithParent = new HashSet<GraphReference>();
                    byParent.Add(instance.Parent, instancesWithParent);
                }

                if (instancesWithParent.Add(instance))
                {
                    // Debug.Log($"Added parent instance mapping:\n{instance.parent.ToSafeString()} => {instance}");
                }
                else
                {
                    Console.WriteLine($"Attempting to add duplicate parent instance mapping:\n{instance.Parent} => {instance}");
                }
            }
        }

        public static void Uninstantiate(GraphReference instance)
        {
            lock (@lock)
            {
                instance.Graph.Uninstantiate(instance);

                if (!byGraph.TryGetValue(instance.Graph, out var instancesWithGraph))
                {
                    throw new InvalidOperationException("Graph instance not found via graph.");
                }

                if (instancesWithGraph.Remove(instance))
                {
                    // Debug.Log($"Removed graph instance mapping:\n{instance.graph} => {instance}");

                    // Free the key references for GC collection
                    if (instancesWithGraph.Count == 0)
                    {
                        byGraph.Remove(instance.Graph);
                    }
                }
                else
                {
                    Console.WriteLine($"Could not find graph instance mapping to remove:\n{instance.Graph} => {instance}");
                }

                if (!byParent.TryGetValue(instance.Parent, out var instancesWithParent))
                {
                    throw new InvalidOperationException("Graph instance not found via parent.");
                }

                if (instancesWithParent.Remove(instance))
                {
                    // Debug.Log($"Removed parent instance mapping:\n{instance.parent.ToSafeString()} => {instance}");

                    // Free the key references for GC collection
                    if (instancesWithParent.Count == 0)
                    {
                        byParent.Remove(instance.Parent);
                    }
                }
                else
                {
                    Console.WriteLine($"Could not find parent instance mapping to remove:\n{instance.Parent} => {instance}");
                }

                // It's important to only free the graph data after
                // dissociating the instance mapping, because the data
                // is used as part of the equality comparison for pointers
                instance.FreeGraphData();
            }
        }

        public static HashSet<GraphReference> OfPooled(IGraph graph)
        {
            Ensure.Ensure.That(nameof(graph)).IsNotNull(graph);

            lock (@lock)
            {
                if (byGraph.TryGetValue(graph, out var instances))
                {
                    // Debug.Log($"Found {instances.Count} instances of {graph}\n{instances.ToLineSeparatedString()}");

                    return instances.ToHashSetPooled();
                }
                else
                {
                    // Debug.Log($"Found no instances of {graph}.\n");

                    return HashSetPool<GraphReference>.New();
                }
            }
        }

        public static HashSet<GraphReference> ChildrenOfPooled(IGraphParent parent)
        {
            Ensure.Ensure.That(nameof(parent)).IsNotNull(parent);

            lock (@lock)
            {
                if (byParent.TryGetValue(parent, out var instances))
                {
                    // Debug.Log($"Found {instances.Count} instances of {parent.ToSafeString()}\n{instances.ToLineSeparatedString()}");

                    return instances.ToHashSetPooled();
                }
                else
                {
                    // Debug.Log($"Found no instances of {parent.ToSafeString()}.\n");

                    return HashSetPool<GraphReference>.New();
                }
            }
        }
    }
}
