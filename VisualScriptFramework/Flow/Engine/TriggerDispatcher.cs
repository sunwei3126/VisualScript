using System;
using System.Collections.Generic;
using System.Linq;
using IoTLogic.Domain;

namespace IoTLogic.Flow.Engine
{
    /// <summary>
    /// Central dispatcher that receives <see cref="DeviceEvent"/>s from external sources
    /// (MQTT broker, HTTP webhooks, timers, etc.) and routes each event to every
    /// registered <see cref="LogicGraphRunner"/> whose trigger nodes match.
    /// </summary>
    public sealed class TriggerDispatcher : IDisposable
    {
        private readonly List<LogicGraphRunner> _runners = new List<LogicGraphRunner>();
        private readonly IDeviceRegistry _registry;
        private bool _running;
        private bool _disposed;

        /// <summary>
        /// Raised after every execution, whether successful or not.
        /// Useful for monitoring, metrics, and command dispatching.
        /// </summary>
        public event Action<ExecutionResult> OnExecuted;

        /// <summary>
        /// Raised when the dispatcher encounters an unhandled exception outside
        /// of graph execution (e.g., device lookup failure).
        /// </summary>
        public event Action<Exception> OnError;

        public TriggerDispatcher(IDeviceRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        // ©¤©¤ Runner management ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        /// <summary>
        /// Registers a logic graph. If the dispatcher is already running the
        /// graph is started immediately.
        /// </summary>
        public void Register(LogicGraphRunner runner)
        {
            if (runner == null) throw new ArgumentNullException(nameof(runner));
            if (_runners.Contains(runner)) return;

            _runners.Add(runner);

            if (_running)
                runner.Start();
        }

        /// <summary>Registers a <see cref="LogicGraph"/> by wrapping it in a runner.</summary>
        public LogicGraphRunner Register(LogicGraph graph, string name = null)
        {
            var runner = new LogicGraphRunner(graph, name);
            Register(runner);
            return runner;
        }

        /// <summary>Removes and stops a previously registered runner.</summary>
        public void Unregister(LogicGraphRunner runner)
        {
            if (runner == null) return;
            _runners.Remove(runner);
            if (_running) runner.Stop();
        }

        // ©¤©¤ Lifecycle ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        /// <summary>Starts all registered runners so they are ready to receive events.</summary>
        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TriggerDispatcher));
            if (_running) return;

            foreach (var runner in _runners)
                runner.Start();

            _running = true;
        }

        /// <summary>Stops all runners without releasing registrations.</summary>
        public void Stop()
        {
            if (!_running) return;

            foreach (var runner in _runners)
                runner.Stop();

            _running = false;
        }

        // ©¤©¤ Dispatch ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        /// <summary>
        /// Dispatches a raw <see cref="DeviceEvent"/> to all matching graphs.
        /// Resolves the device from the registry, builds a <see cref="TriggerContext"/>,
        /// and calls <see cref="Execute"/> on every matching runner.
        /// </summary>
        public void Dispatch(DeviceEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            if (!_running)
            {
                Console.WriteLine("[TriggerDispatcher] Dispatch called before Start() ¡ª event ignored.");
                return;
            }

            try
            {
                if (!_registry.TryGetDevice(@event.DeviceId, out var device))
                {
                    Console.WriteLine($"[TriggerDispatcher] Device not found: {@event.DeviceId}");
                    return;
                }

                var ctx = new TriggerContext(@event, device);
                DispatchContext(ctx);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                Console.WriteLine($"[TriggerDispatcher] Unhandled error: {ex}");
            }
        }

        /// <summary>
        /// Dispatches a pre-built <see cref="TriggerContext"/> directly to all runners.
        /// Use this when you have already resolved the device and built the context.
        /// </summary>
        public void DispatchContext(TriggerContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            var snapshot = _runners.ToList();   // safe copy ¡ª runner list may change

            foreach (var runner in snapshot)
            {
                var result = runner.Execute(ctx);
                OnExecuted?.Invoke(result);

                // Dispatch any commands that were enqueued during execution
                if (result.Succeeded && result.Commands.Count > 0)
                    DispatchCommands(result.Commands, ctx);
            }
        }

        // ©¤©¤ Command dispatching ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        /// <summary>
        /// Override or subscribe to handle outbound device commands.
        /// Default behaviour writes commands to the console.
        /// </summary>
        public Action<IReadOnlyList<DeviceCommand>, TriggerContext> CommandHandler { get; set; }

        private void DispatchCommands(
            IReadOnlyList<DeviceCommand> commands,
            TriggerContext ctx)
        {
            if (CommandHandler != null)
            {
                CommandHandler(commands, ctx);
                return;
            }

            // Built-in fallback: log to console
            foreach (var cmd in commands)
                Console.WriteLine($"[TriggerDispatcher] CMD ¡ú {cmd}");
        }

        // ©¤©¤ IDisposable ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            foreach (var runner in _runners)
                runner.Dispose();
            _runners.Clear();
            _disposed = true;
        }
    }
}
