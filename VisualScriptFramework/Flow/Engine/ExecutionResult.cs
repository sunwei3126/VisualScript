using System;
using System.Collections.Generic;
using IoTLogic.Domain;

namespace IoTLogic.Flow.Engine
{
    /// <summary>
    /// Captures the outcome of a single logic graph execution pass.
    /// </summary>
    public sealed class ExecutionResult
    {
        internal ExecutionResult() { }

        /// <summary>True when the graph completed without throwing an exception.</summary>
        public bool Succeeded { get; internal set; }

        /// <summary>Exception thrown during execution, or null on success.</summary>
        public Exception Error { get; internal set; }

        /// <summary>The <see cref="TriggerContext"/> used for this execution.</summary>
        public TriggerContext Context { get; internal set; }

        /// <summary>All commands enqueued by Action nodes during this execution.</summary>
        public IReadOnlyList<DeviceCommand> Commands => Context?.PendingCommands ?? _empty;

        /// <summary>Wall-clock duration of the execution.</summary>
        public TimeSpan Duration { get; internal set; }

        private static readonly IReadOnlyList<DeviceCommand> _empty =
            new System.Collections.ObjectModel.ReadOnlyCollection<DeviceCommand>(new List<DeviceCommand>());

        public override string ToString() =>
            Succeeded
                ? $"OK  [{Duration.TotalMilliseconds:F1}ms]  commands={Commands.Count}"
                : $"ERR [{Duration.TotalMilliseconds:F1}ms]  {Error?.Message}";
    }
}
