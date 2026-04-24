using System;
using System.Collections.Generic;

namespace IoTLogic.Domain
{
    /// <summary>
    /// Execution context passed through the logic graph when a trigger fires.
    /// Carries the originating event, the resolved device, and a shared data bag
    /// that nodes can use to pass results downstream.
    /// </summary>
    public sealed class TriggerContext
    {
        public TriggerContext(DeviceEvent @event, IDevice device)
        {
            Event  = @event ?? throw new ArgumentNullException(nameof(@event));
            Device = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>The device event that triggered this execution.</summary>
        public DeviceEvent Event { get; }

        /// <summary>The device that emitted the event.</summary>
        public IDevice Device { get; }

        /// <summary>
        /// Shared data bag for passing intermediate values between nodes
        /// within a single execution pass (keyed by node-defined string keys).
        /// </summary>
        public Dictionary<string, object> Data { get; } = new Dictionary<string, object>();

        /// <summary>Commands accumulated by action nodes during execution.</summary>
        public List<DeviceCommand> PendingCommands { get; } = new List<DeviceCommand>();

        /// <summary>UTC timestamp when this execution was started.</summary>
        public DateTime StartedAt { get; } = DateTime.UtcNow;

        /// <summary>Enqueues a command to be dispatched after graph execution completes.</summary>
        public void EnqueueCommand(DeviceCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            PendingCommands.Add(command);
        }

        /// <summary>Stores an intermediate value into the shared data bag.</summary>
        public void Set(string key, object value) => Data[key] = value;

        /// <summary>Retrieves an intermediate value from the shared data bag.</summary>
        public bool TryGet(string key, out object value) => Data.TryGetValue(key, out value);

        /// <summary>Retrieves a typed intermediate value from the shared data bag.</summary>
        public T Get<T>(string key)
        {
            if (Data.TryGetValue(key, out var raw) && raw is T typed)
                return typed;
            throw new KeyNotFoundException($"TriggerContext data key not found: '{key}'");
        }

        public override string ToString() =>
            $"TriggerContext[{Event.DeviceId}/{Event.EventName} @ {StartedAt:O}]";
    }
}
