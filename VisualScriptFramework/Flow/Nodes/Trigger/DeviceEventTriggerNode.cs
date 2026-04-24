using System;
using IoTLogic.Core.Events;
using IoTLogic.Core.Graph;
using IoTLogic.Domain;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Trigger
{
    /// <summary>
    /// Entry point of a logic graph. Fires when a matching device event arrives.
    /// Outputs the DeviceEvent payload and the originating TriggerContext.
    /// </summary>
    public sealed class DeviceEventTriggerNode : LogicNode, IGraphEventListener
    {
        // ©¤©¤ Outputs ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public ControlOutput triggered  { get; private set; }
        public ValueOutput   deviceId   { get; private set; }
        public ValueOutput   eventName  { get; private set; }
        public ValueOutput   payload    { get; private set; }
        public ValueOutput   context    { get; private set; }

        // ©¤©¤ Configurable filter inputs ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>Optional filter: only fire for this device ID (leave empty for any).</summary>
        public ValueInput filterDeviceId  { get; private set; }

        /// <summary>Optional filter: only fire for this event name (leave empty for any).</summary>
        public ValueInput filterEventName { get; private set; }

        protected override void Definition()
        {
            IsControlRoot = true;

            filterDeviceId  = ValueInput<string>(nameof(filterDeviceId),  string.Empty);
            filterEventName = ValueInput<string>(nameof(filterEventName), string.Empty);

            triggered = ControlOutput(nameof(triggered));
            deviceId  = ValueOutput<string>(nameof(deviceId),  GetDeviceId);
            eventName = ValueOutput<string>(nameof(eventName), GetEventName);
            payload   = ValueOutput<object>(nameof(payload),   GetPayload);
            context   = ValueOutput<TriggerContext>(nameof(context), GetContext);
        }

        // ©¤©¤ IGraphEventListener ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public void StartListening(GraphStack stack)
        {
            stack.GetGraphData<LogicGraphData>().IsListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            stack.GetGraphData<LogicGraphData>().IsListening = false;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetGraphData<LogicGraphData>().IsListening;
        }

        // ©¤©¤ Public trigger API (called by ExecutionEngine) ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>
        /// Evaluates whether this node should fire for the given event.
        /// Returns true when filter conditions are satisfied.
        /// </summary>
        public bool Matches(DeviceEvent @event, Flow flow)
        {
            var idFilter   = flow.GetValue<string>(filterDeviceId);
            var nameFilter = flow.GetValue<string>(filterEventName);

            if (!string.IsNullOrEmpty(idFilter)   && @event.DeviceId  != idFilter)   return false;
            if (!string.IsNullOrEmpty(nameFilter)  && @event.EventName != nameFilter) return false;
            return true;
        }

        // ©¤©¤ Value resolvers (called lazily when downstream nodes pull values) ©¤
        private string GetDeviceId(Flow flow)
        {
            var ctx = GetTriggerContext(flow);
            return ctx?.Event.DeviceId ?? string.Empty;
        }

        private string GetEventName(Flow flow)
        {
            var ctx = GetTriggerContext(flow);
            return ctx?.Event.EventName ?? string.Empty;
        }

        private object GetPayload(Flow flow)
        {
            var ctx = GetTriggerContext(flow);
            return ctx?.Event.Payload;
        }

        private TriggerContext GetContext(Flow flow)
        {
            return GetTriggerContext(flow);
        }

        private TriggerContext GetTriggerContext(Flow flow)
        {
            if (flow.variables.IsDefined(TriggerContextKey))
                return flow.variables.Get<TriggerContext>(TriggerContextKey);
            return null;
        }

        /// <summary>Variable key used to store the TriggerContext in the flow.</summary>
        public const string TriggerContextKey = "__iotTriggerContext__";
    }
}
