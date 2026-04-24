using System;
using System.Collections.Generic;

namespace IoTLogic.Domain
{
    /// <summary>
    /// Represents an event reported by an IoT device (e.g., alarm, status change, sensor trigger).
    /// </summary>
    public sealed class DeviceEvent
    {
        public DeviceEvent(
            string deviceId,
            string productKey,
            string eventName,
            object payload,
            DateTime timestamp)
        {
            DeviceId   = deviceId   ?? throw new ArgumentNullException(nameof(deviceId));
            ProductKey = productKey ?? throw new ArgumentNullException(nameof(productKey));
            EventName  = eventName  ?? throw new ArgumentNullException(nameof(eventName));
            Payload    = payload;
            Timestamp  = timestamp;
        }

        /// <summary>ID of the device that emitted this event.</summary>
        public string DeviceId { get; }

        /// <summary>Product/model key of the device.</summary>
        public string ProductKey { get; }

        /// <summary>Event identifier (e.g., "TemperatureAlarm", "DoorOpened").</summary>
        public string EventName { get; }

        /// <summary>
        /// Event payload. May be a primitive value, a <see cref="DeviceProperty"/>,
        /// or a <c>Dictionary&lt;string, object&gt;</c> for multi-field events.
        /// </summary>
        public object Payload { get; }

        /// <summary>UTC timestamp when the event occurred.</summary>
        public DateTime Timestamp { get; }

        /// <summary>Returns the payload cast to the specified type.</summary>
        public T GetPayload<T>()
        {
            if (Payload is T typed)
                return typed;
            return (T)Convert.ChangeType(Payload, typeof(T));
        }

        /// <summary>
        /// Returns a payload field by name when the payload is a dictionary.
        /// </summary>
        public bool TryGetField(string fieldName, out object value)
        {
            if (Payload is IDictionary<string, object> dict)
                return dict.TryGetValue(fieldName, out value);
            value = null;
            return false;
        }

        public override string ToString() =>
            $"[{Timestamp:O}] {DeviceId}/{EventName}: {Payload}";
    }
}
