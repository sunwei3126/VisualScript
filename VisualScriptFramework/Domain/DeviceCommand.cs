using System;
using System.Collections.Generic;

namespace IoTLogic.Domain
{
    /// <summary>
    /// Represents a command to be sent to an IoT device (e.g., turn on, set temperature).
    /// </summary>
    public sealed class DeviceCommand
    {
        public DeviceCommand(string deviceId, string commandName)
            : this(deviceId, commandName, new Dictionary<string, object>()) { }

        public DeviceCommand(
            string deviceId,
            string commandName,
            IDictionary<string, object> parameters)
        {
            DeviceId    = deviceId    ?? throw new ArgumentNullException(nameof(deviceId));
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
            Parameters  = new Dictionary<string, object>(parameters ?? new Dictionary<string, object>());
        }

        /// <summary>Target device ID.</summary>
        public string DeviceId { get; }

        /// <summary>Command name (e.g., "SetTemperature", "TurnOff").</summary>
        public string CommandName { get; }

        /// <summary>Command parameters (key/value pairs).</summary>
        public IReadOnlyDictionary<string, object> Parameters { get; }

        /// <summary>UTC time this command was created.</summary>
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        /// <summary>Fluent helper to add a parameter.</summary>
        public DeviceCommand WithParameter(string key, object value)
        {
            var copy = new Dictionary<string, object>();
            foreach (var kv in Parameters) copy[kv.Key] = kv.Value;
            copy[key] = value;
            return new DeviceCommand(DeviceId, CommandName, copy);
        }

        public override string ToString() =>
            $"CMD {DeviceId}/{CommandName}({string.Join(", ", Parameters)})";
    }
}
