using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Utities;
using VisualScript.Flow.Connections;

namespace VisualScript.Flow.Ports
{
    public sealed class ControlOutput : UnitPort<ControlInput, IUnitInputPort, ControlConnection>, IUnitControlPort, IUnitOutputPort
    {
        public ControlOutput(string key) : base(key) { }

        public override IEnumerable<ControlConnection> ValidConnections => Unit?.Graph?.ControlConnections.WithSource(this) ?? Enumerable.Empty<ControlConnection>();

        public override IEnumerable<InvalidConnection> InvalidConnections => Unit?.Graph?.InvalidConnections.WithSource(this) ?? Enumerable.Empty<InvalidConnection>();

        public override IEnumerable<ControlInput> ValidConnectedPorts => ValidConnections.Select(c => c.Destination);

        public override IEnumerable<IUnitInputPort> InvalidConnectedPorts => InvalidConnections.Select(c => c.Destination);

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
            if (Unit.IsControlRoot)
            {
                return true;
            }

            if (!recursion?.TryEnter(this) ?? false)
            {
                return false;
            }

            var isPredictable = Unit.Relations.WithDestination(this).Where(r => r.Source is ControlInput).All(r => ((ControlInput)r.Source).Predictable(recursion));
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

                if (Unit.IsControlRoot)
                {
                    return true;
                }

                return Unit.Relations.WithDestination(this).Where(r => r.Source is ControlInput).Any(r => ((ControlInput)r.Source).CouldBeEntered);
            }
        }

        public ControlConnection Connection => Unit.Graph?.ControlConnections.SingleOrDefaultWithSource(this);

        public override bool HasValidConnection => Connection != null;

        public override bool CanConnectToValid(ControlInput port)
        {
            return true;
        }

        public override void ConnectToValid(ControlInput port)
        {
            var source = this;
            var destination = port;

            source.Disconnect();

            Unit.Graph.ControlConnections.Add(new ControlConnection(source, destination));
        }

        public override void ConnectToInvalid(IUnitInputPort port)
        {
            ConnectInvalid(this, port);
        }

        public override void DisconnectFromValid(ControlInput port)
        {
            var connection = ValidConnections.SingleOrDefault(c => c.Destination == port);

            if (connection != null)
            {
                Unit.Graph.ControlConnections.Remove(connection);
            }
        }

        public override void DisconnectFromInvalid(IUnitInputPort port)
        {
            DisconnectInvalid(this, port);
        }

        public override IUnitPort CompatiblePort(IUnit unit)
        {
            if (unit == this.Unit) return null;
            return unit.ControlInputs.FirstOrDefault();
        }
    }
}
