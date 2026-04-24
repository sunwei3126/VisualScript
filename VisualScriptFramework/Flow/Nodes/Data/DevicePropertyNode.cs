using IoTLogic.Domain;
using IoTLogic.Flow.Nodes.Trigger;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Data
{
    /// <summary>
    /// Reads a named property from the device that triggered the current execution.
    /// The property value is resolved lazily when a downstream node pulls it.
    /// </summary>
    public sealed class DevicePropertyNode : LogicNode
    {
        // ©¤©¤ Inputs ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>
        /// Property name to read (e.g., "temperature", "humidity").
        /// When left empty the raw event payload is returned.
        /// </summary>
        public ValueInput propertyName { get; private set; }

        // ©¤©¤ Outputs ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>The raw property value (object).</summary>
        public ValueOutput value      { get; private set; }

        /// <summary>Numeric representation of the value (double).</summary>
        public ValueOutput numericValue { get; private set; }

        /// <summary>String representation of the value.</summary>
        public ValueOutput stringValue  { get; private set; }

        /// <summary>True when the property exists and is not null.</summary>
        public ValueOutput exists       { get; private set; }

        protected override void Definition()
        {
            propertyName = ValueInput<string>(nameof(propertyName), string.Empty);

            value        = ValueOutput<object>(nameof(value),        GetValue);
            numericValue = ValueOutput<double>(nameof(numericValue), GetNumericValue);
            stringValue  = ValueOutput<string>(nameof(stringValue),  GetStringValue);
            exists       = ValueOutput<bool>(nameof(exists),         GetExists);

            Requirement(propertyName, value);
            Requirement(propertyName, numericValue);
            Requirement(propertyName, stringValue);
            Requirement(propertyName, exists);
        }

        private object GetValue(Flow flow)
        {
            var ctx = GetContext(flow);
            if (ctx == null) return null;

            var name = flow.GetValue<string>(propertyName);
            if (string.IsNullOrEmpty(name))
                return ctx.Event.Payload;

            return ctx.Device.HasProperty(name)
                ? ctx.Device.GetProperty(name)
                : null;
        }

        private double GetNumericValue(Flow flow)
        {
            var raw = GetValue(flow);
            if (raw == null) return 0.0;
            try { return System.Convert.ToDouble(raw); }
            catch { return 0.0; }
        }

        private string GetStringValue(Flow flow) => GetValue(flow)?.ToString() ?? string.Empty;

        private bool GetExists(Flow flow) => GetValue(flow) != null;

        private TriggerContext GetContext(Flow flow)
        {
            if (flow.variables.IsDefined(DeviceEventTriggerNode.TriggerContextKey))
                return flow.variables.Get<TriggerContext>(DeviceEventTriggerNode.TriggerContextKey);
            return null;
        }
    }
}
