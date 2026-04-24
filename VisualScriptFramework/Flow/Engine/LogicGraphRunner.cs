using System;
using System.Linq;
using IoTLogic.Core.Graph;
using IoTLogic.Domain;
using IoTLogic.Flow.Nodes.Trigger;

namespace IoTLogic.Flow.Engine
{
    /// <summary>
    /// Manages the lifecycle (instantiation / teardown) of one <see cref="LogicGraph"/>
    /// and executes it in response to a <see cref="TriggerContext"/>.
    /// </summary>
    public sealed class LogicGraphRunner : IDisposable
    {
        private readonly LogicGraphMachine _machine;
        private GraphReference _reference;
        private bool _disposed;

        public LogicGraph Graph { get; }

        /// <summary>Optional display name for logging/diagnostics.</summary>
        public string Name { get; set; }

        public LogicGraphRunner(LogicGraph graph, string name = null)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            Name  = name ?? graph.Title ?? "LogicGraph";
            _machine = new LogicGraphMachine(graph);
        }

        // ©¤©¤ Lifecycle ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        /// <summary>
        /// Instantiates the graph so event listeners and node state are active.
        /// Must be called before <see cref="Execute"/>.
        /// </summary>
        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LogicGraphRunner));
            if (_reference != null) return;   // already started

            _reference = GraphReference.New(_machine, ensureValid: true);
            GraphInstances.Instantiate(_reference);
        }

        /// <summary>
        /// Deactivates the graph and releases all instance data.
        /// </summary>
        public void Stop()
        {
            if (_reference == null) return;
            GraphInstances.Uninstantiate(_reference);
            _reference = null;
        }

        // ©¤©¤ Execution ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        /// <summary>
        /// Runs the graph for the given <paramref name="triggerContext"/>.
        /// Finds all <see cref="DeviceEventTriggerNode"/>s whose filters match the
        /// event, injects the context into the flow variables, and fires each one.
        /// </summary>
        public ExecutionResult Execute(TriggerContext triggerContext)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LogicGraphRunner));
            if (_reference == null)
                throw new InvalidOperationException($"Runner '{Name}' has not been started. Call Start() first.");

            var result = new ExecutionResult { Context = triggerContext };
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var triggerNodes = Graph.LogicNodes
                    .OfType<DeviceEventTriggerNode>()
                    .ToList();

                if (triggerNodes.Count == 0)
                {
                    result.Succeeded = true;
                    return result;
                }

                foreach (var triggerNode in triggerNodes)
                {
                    FireTrigger(triggerNode, triggerContext);
                }

                result.Succeeded = true;
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Error = ex;
                Console.WriteLine($"[LogicGraphRunner:{Name}] Execution error: {ex}");
            }
            finally
            {
                sw.Stop();
                result.Duration = sw.Elapsed;
            }

            return result;
        }

        private void FireTrigger(DeviceEventTriggerNode triggerNode, TriggerContext ctx)
        {
            var flow = Flow.New(_reference);

            // Inject TriggerContext so all downstream nodes can read it
            flow.variables.Set(DeviceEventTriggerNode.TriggerContextKey, ctx);

            // Check filter ¡ª evaluate with the live flow so filter inputs
            // can themselves be connected to data nodes
            if (!triggerNode.Matches(ctx.Event, flow))
            {
                flow.Dispose();
                return;
            }

            // Fire the triggered control output
            if (triggerNode.triggered.HasValidConnection)
            {
                flow.Run(triggerNode.triggered);   // Run disposes flow when done
            }
            else
            {
                flow.Dispose();
            }
        }

        // ©¤©¤ IDisposable ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
        }
    }
}
