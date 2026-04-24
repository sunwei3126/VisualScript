using System.Collections.Generic;

namespace IoTLogic.Domain
{
    /// <summary>
    /// Registry for looking up devices by ID or product key.
    /// Implementations may resolve devices from cloud APIs, local caches, or test fakes.
    /// </summary>
    public interface IDeviceRegistry
    {
        /// <summary>Retrieves a device by its unique ID. Returns null if not found.</summary>
        IDevice GetDevice(string deviceId);

        /// <summary>Returns all devices belonging to the given product key.</summary>
        IEnumerable<IDevice> GetDevicesByProduct(string productKey);

        /// <summary>Returns true if a device with the given ID is registered.</summary>
        bool TryGetDevice(string deviceId, out IDevice device);
    }
}
