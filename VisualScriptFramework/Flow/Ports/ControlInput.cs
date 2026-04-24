using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IoTLogic.Core.Ensure;
using IoTLogic.Core.Utities;
using IoTLogic.Flow.Connections;

namespace IoTLogic.Flow.Ports
{
    public sealed class ControlInput : UnitPort<ControlOutput, IUnitOutputPort, ControlConnection>, IUnitControlPort, IUnitInputPort
    {
        public ControlInput(string key, Func<Flow, ControlOutput> action) : base(key)
        {
            Ensure.That(nameof(action)).IsNotNull(action);
            this.action = action;
        }

        public ControlInput(string key, Func<Flow, IEnumerator> coroutineAction) : base(key)
        {
            Ensure.That(nameof(coroutineAction)).IsNotNull(coroutineAction);

            this.coroutineAction = coroutineAction;
        }

        public ControlInput(string key, Func<Flow, ControlOutput> action, Func<Flow, IEnumerator> coroutineAction) : base(key)
        {
            Ensure.That(nameof(action)).IsNotNull(action);
            Ensure.That(nameof(coroutineAction)).IsNotNull(coroutineAction);

            this.action = action;
            this.coroutineAction = coroutineAction;
        }

        public bool supportsCoroutine => coroutineAction != null;

        public bool requiresCoroutine => action == null;

        internal readonly Func<Flow, ControlOutput> action;

        internal readonly Func<Flow, IEnumerator> coroutineAction;

        public override IEnumerable<ControlConnection> ValidConnections => LogicNode?.Graph?.ControlConnections.WithDestination(this) ?? Enumerable.Empty<ControlConnection>();

        public override IEnumerable<InvalidConnection> InvalidConnections => LogicNode?.Graph?.InvalidConnections.WithDestination(this) ?? Enumerable.Empty<InvalidConnection>();

        public override IEnumerable<ControlOutput> ValidConnectedPorts => ValidConnections.Select(c => c.Source);

        public override IEnumerable<IUnitOutputPort> InvalidConnectedPorts => InvalidConnections.Select(c => c.Source);

        public bool IsPredictable
        {
            get
            {
                using (var recursion = Recursion.New(1))
                {
                    return Predictable(recursion);
                }
            }
        }

        public bool Predictable(Recursion recursion)
        {
            if (!HasValidConnection)
            {
                return true;
            }

            if (!recursion?.TryEnter(this) ?? false)
            {
                return false;
            }

            var isPredictable = ValidConnectedPorts.All(cop => cop.Predictable(recursion));
            recursion?.Exit(this);
            return isPredictable;
        }

        public bool CouldBeEntered
        {
            get
            {
                if (!IsPredictable)
                {
                    throw new NotSupportedException();
                }

                if (!HasValidConnection)
                {
                    return false;
                }

                return ValidConnectedPorts.Any(cop => cop.CouldBeEntered);
            }
        }

        public override bool CanConnectToValid(ControlOutput port)
        {
            return true;
        }

        public override void ConnectToValid(ControlOutput port)
        {
            var source = port;
            var destination = this;

            source.Disconnect();

            LogicNode.Graph.ControlConnections.Add(new ControlConnection(source, destination));
        }

        public override void ConnectToInvalid(IUnitOutputPort port)
        {
            ConnectInvalid(port, this);
        }

        public override void DisconnectFromValid(ControlOutput port)
        {
            var connection = ValidConnections.SingleOrDefault(c => c.Source == port);

            if (connection != null)
            {
                LogicNode.Graph.ControlConnections.Remove(connection);
            }
        }

        public override void DisconnectFromInvalid(IUnitOutputPort port)
        {
            DisconnectInvalid(port, this);
        }

        public override IUnitPort CompatiblePort(ILogicNode LogicNode)
        {
            if (LogicNode == this.LogicNode) return null;
            return LogicNode.ControlOutputs.FirstOrDefault();
        }
    }
}
