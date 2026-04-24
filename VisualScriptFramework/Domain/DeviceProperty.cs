using System;

namespace IoTLogic.Domain
{
    /// <summary>
    /// Represents a single property reported by a device (e.g., temperature, humidity).
    /// </summary>
    public sealed class DeviceProperty
    {
        public DeviceProperty(string name, object value, DateTime reportedAt)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
            ReportedAt = reportedAt;
        }

        /// <summary>Property identifier (e.g., "temperature").</summary>
        public string Name { get; }

        /// <summary>Current reported value.</summary>
        public object Value { get; }

        /// <summary>Timestamp when this value was last reported.</summary>
        public DateTime ReportedAt { get; }

        /// <summary>Returns the value cast to the specified type.</summary>
        public T GetValue<T>()
        {
            if (Value is T typed)
                return typed;
            return (T)Convert.ChangeType(Value, typeof(T));
        }

        public override string ToString() => $"{Name}={Value} @ {ReportedAt:O}";
    }
}
