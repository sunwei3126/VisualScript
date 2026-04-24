using System.Collections.Generic;
using IoTLogic.Domain;
using IoTLogic.Flow.Nodes.Trigger;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Action
{
    /// <summary>
    /// Builds a <see cref="DeviceCommand"/> and enqueues it onto the
    /// <see cref="TriggerContext.PendingCommands"/> list for dispatch after
    /// graph execution completes.
    /// </summary>
    public sealed class SendCommandNode : LogicNode
    {
        // ©¤©¤ Control ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public ControlInput  enter { get; private set; }
        public ControlOutput exit  { get; private set; }

        // ©¤©¤ Command inputs ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>Target device ID. Defaults to the event's device when left empty.</summary>
        public ValueInput targetDeviceId { get; private set; }

        /// <summary>Command name to send (e.g., "TurnOff", "SetTemperature").</summary>
        public ValueInput commandName    { get; private set; }

        /// <summary>Optional parameter key 1.</summary>
        public ValueInput paramKey1   { get; private set; }
        public ValueInput paramValue1 { get; private set; }

        /// <summary>Optional parameter key 2.</summary>
        public ValueInput paramKey2   { get; private set; }
        public ValueInput paramValue2 { get; private set; }

        /// <summary>The built command (available for inspection downstream).</summary>
        public ValueOutput builtCommand { get; private set; }

        private DeviceCommand _lastCommand;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Execute);

            targetDeviceId = ValueInput<string>(nameof(targetDeviceId), string.Empty);
            commandName    = ValueInput<string>(nameof(commandName),    string.Empty);
            paramKey1      = ValueInput<string>(nameof(paramKey1),      string.Empty);
            paramValue1    = ValueInput<object>(nameof(paramValue1),    null);
            paramKey2      = ValueInput<string>(nameof(paramKey2),      string.Empty);
            paramValue2    = ValueInput<object>(nameof(paramValue2),    null);

            exit        = ControlOutput(nameof(exit));
            builtCommand = ValueOutput<DeviceCommand>(nameof(builtCommand), _ => _lastCommand);

            Requirement(targetDeviceId, enter);
            Requirement(commandName,    enter);
            Succession(enter, exit);
        }

        private ControlOutput Execute(Flow flow)
        {
            // Resolve target device ID ¡ª fall back to event's device when empty
            var devId = flow.GetValue<string>(targetDeviceId);
            if (string.IsNullOrEmpty(devId))
            {
                var ctx = GetContext(flow);
                devId = ctx?.Event.DeviceId ?? string.Empty;
            }

            var cmd  = flow.GetValue<string>(commandName);
            var prms = new Dictionary<string, object>();

            var k1 = flow.GetValue<string>(paramKey1);
            if (!string.IsNullOrEmpty(k1))
                prms[k1] = flow.GetValue<object>(paramValue1);

            var k2 = flow.GetValue<string>(paramKey2);
            if (!string.IsNullOrEmpty(k2))
                prms[k2] = flow.GetValue<object>(paramValue2);

            _lastCommand = new DeviceCommand(devId, cmd, prms);

            // Enqueue onto TriggerContext if present
            GetContext(flow)?.EnqueueCommand(_lastCommand);

            return exit;
        }

        private TriggerContext GetContext(Flow flow)
        {
            if (flow.variables.IsDefined(DeviceEventTriggerNode.TriggerContextKey))
                return flow.variables.Get<TriggerContext>(DeviceEventTriggerNode.TriggerContextKey);
            return null;
        }
    }
}
