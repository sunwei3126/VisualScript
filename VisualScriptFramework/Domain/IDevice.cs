using System.Collections.Generic;

namespace IoTLogic.Domain
{
    /// <summary>
    /// Represents a physical or virtual IoT device.
    /// </summary>
    public interface IDevice
    {
        /// <summary>Unique device identifier.</summary>
        string DeviceId { get; }

        /// <summary>Product/model key the device belongs to.</summary>
        string ProductKey { get; }

        /// <summary>Human-readable display name.</summary>
        string DisplayName { get; }

        /// <summary>Current online/offline status.</summary>
        DeviceStatus Status { get; }

        /// <summary>All reported properties of this device, keyed by property name.</summary>
        IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>Returns a property value by name, or null if not present.</summary>
        object GetProperty(string name);

        /// <summary>Returns a typed property value by name.</summary>
        T GetProperty<T>(string name);

        /// <summary>Returns true if the device has the given property.</summary>
        bool HasProperty(string name);
    }

    public enum DeviceStatus
    {
        Unknown,
        Online,
        Offline,
        Inactive
    }
}
