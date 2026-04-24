using System;
using System.Collections.Generic;
using System.Linq;
using IoTLogic.Domain;

namespace IoTLogic.Flow.Engine
{
    /// <summary>
    /// Simple in-memory implementation of <see cref="IDeviceRegistry"/>.
    /// Suitable for unit tests, local development, and simulation scenarios.
    /// </summary>
    public sealed class InMemoryDeviceRegistry : IDeviceRegistry
    {
        private readonly Dictionary<string, IDevice> _devices =
            new Dictionary<string, IDevice>(StringComparer.OrdinalIgnoreCase);

        // ©¤©¤ Registration ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        public void AddDevice(IDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            _devices[device.DeviceId] = device;
        }

        /// <summary>
        /// Convenience method: adds a <see cref="SimpleDevice"/> from plain values.
        /// </summary>
        public SimpleDevice AddDevice(
            string deviceId,
            string productKey,
            string displayName = null,
            DeviceStatus status = DeviceStatus.Online)
        {
            var device = new SimpleDevice(deviceId, productKey, displayName ?? deviceId, status);
            _devices[deviceId] = device;
            return device;
        }

        public void RemoveDevice(string deviceId) => _devices.Remove(deviceId);

        public void Clear() => _devices.Clear();

        // ©¤©¤ IDeviceRegistry ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        public IDevice GetDevice(string deviceId) =>
            _devices.TryGetValue(deviceId, out var d) ? d : null;

        public bool TryGetDevice(string deviceId, out IDevice device) =>
            _devices.TryGetValue(deviceId, out device);

        public IEnumerable<IDevice> GetDevicesByProduct(string productKey) =>
            _devices.Values.Where(d =>
                string.Equals(d.ProductKey, productKey, StringComparison.OrdinalIgnoreCase));
    }

    // ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
    // Minimal concrete device ¡ª used by InMemoryDeviceRegistry and unit tests
    // ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// Mutable, in-memory device implementation for testing and simulation.
    /// </summary>
    public sealed class SimpleDevice : IDevice
    {
        private readonly Dictionary<string, object> _props =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public SimpleDevice(
            string deviceId,
            string productKey,
            string displayName,
            DeviceStatus status = DeviceStatus.Online)
        {
            DeviceId    = deviceId    ?? throw new ArgumentNullException(nameof(deviceId));
            ProductKey  = productKey  ?? throw new ArgumentNullException(nameof(productKey));
            DisplayName = displayName ?? deviceId;
            Status      = status;
        }

        // ©¤©¤ IDevice ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public string DeviceId    { get; }
        public string ProductKey  { get; }
        public string DisplayName { get; set; }
        public DeviceStatus Status { get; set; }

        public IReadOnlyDictionary<string, object> Properties => _props;

        public object GetProperty(string name) =>
            _props.TryGetValue(name, out var v) ? v : null;

        public T GetProperty<T>(string name)
        {
            var v = GetProperty(name);
            if (v is T typed) return typed;
            return (T)Convert.ChangeType(v, typeof(T));
        }

        public bool HasProperty(string name) => _props.ContainsKey(name);

        // ©¤©¤ Mutation helpers ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

        /// <summary>Sets or updates a property value.</summary>
        public SimpleDevice SetProperty(string name, object value)
        {
            _props[name] = value;
            return this;
        }

        public override string ToString() =>
            $"Device[{DeviceId}/{ProductKey} {Status}]";
    }
}
