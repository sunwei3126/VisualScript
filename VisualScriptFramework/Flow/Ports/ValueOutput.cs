using System;
using System.Collections.Generic;
using System.Linq;
using IoTLogic.Core.Ensure;
using IoTLogic.Core.Reflection;
using IoTLogic.Flow.Connections;

namespace IoTLogic.Flow.Ports
{
    public sealed class ValueOutput : UnitPort<ValueInput, IUnitInputPort, ValueConnection>, IUnitValuePort, IUnitOutputPort
    {
        public ValueOutput(string key, Type type, Func<Flow, object> getValue) : base(key)
        {
            Ensure.That(nameof(type)).IsNotNull(type);
            Ensure.That(nameof(getValue)).IsNotNull(getValue);

            this.Type = type;
            this.getValue = getValue;
        }

        public ValueOutput(string key, Type type) : base(key)
        {
            Ensure.That(nameof(type)).IsNotNull(type);

            this.Type = type;
        }

        internal readonly Func<Flow, object> getValue;

        internal Func<Flow, bool> canPredictValue;

        public bool supportsPrediction => canPredictValue != null;

        public bool supportsFetch => getValue != null;

        public Type Type { get; }

        public override IEnumerable<ValueConnection> ValidConnections => LogicNode?.Graph?.ValueConnections.WithSource(this) ?? Enumerable.Empty<ValueConnection>();

        public override IEnumerable<InvalidConnection> InvalidConnections => LogicNode?.Graph?.InvalidConnections.WithSource(this) ?? Enumerable.Empty<InvalidConnection>();

        public override IEnumerable<ValueInput> ValidConnectedPorts => ValidConnections.Select(c => c.Destination);

        public override IEnumerable<IUnitInputPort> InvalidConnectedPorts => InvalidConnections.Select(c => c.Destination);

        public override bool CanConnectToValid(ValueInput port)
        {
            var source = this;
            var destination = port;

            return source.Type.IsConvertibleTo(destination.Type, false);
        }

        public override void ConnectToValid(ValueInput port)
        {
            var source = this;
            var destination = port;

            destination.Disconnect();

            LogicNode.Graph.ValueConnections.Add(new ValueConnection(source, destination));
        }

        public override void ConnectToInvalid(IUnitInputPort port)
        {
            ConnectInvalid(this, port);
        }

        public override void DisconnectFromValid(ValueInput port)
        {
            var connection = ValidConnections.SingleOrDefault(c => c.Destination == port);

            if (connection != null)
            {
                LogicNode.Graph.ValueConnections.Remove(connection);
            }
        }

        public override void DisconnectFromInvalid(IUnitInputPort port)
        {
            DisconnectInvalid(this, port);
        }

        public ValueOutput PredictableIf(Func<Flow, bool> condition)
        {
            Ensure.That(nameof(condition)).IsNotNull(condition);
            canPredictValue = condition;
            return this;
        }

        public ValueOutput Predictable()
        {
            canPredictValue = (flow) => true;
            return this;
        }

        public override IUnitPort CompatiblePort(ILogicNode LogicNode)
        {
            if (LogicNode == this.LogicNode) return null;
            return LogicNode.CompatibleValueInput(Type);
        }
    }
}
