using System.Collections.Generic;
using System.Linq;
using VisualScript.Flow.Connections;

namespace VisualScript.Flow.Ports
{
    public sealed class InvalidOutput : UnitPort<IUnitInputPort, IUnitInputPort, InvalidConnection>, IUnitInvalidPort, IUnitOutputPort
    {
        public InvalidOutput(string key) : base(key) { }

        public override IEnumerable<InvalidConnection> ValidConnections => Unit?.Graph?.InvalidConnections.WithSource(this) ?? Enumerable.Empty<InvalidConnection>();

        public override IEnumerable<InvalidConnection> InvalidConnections => Enumerable.Empty<InvalidConnection>();

        public override IEnumerable<IUnitInputPort> ValidConnectedPorts => ValidConnections.Select(c => c.Destination);

        public override IEnumerable<IUnitInputPort> InvalidConnectedPorts => InvalidConnections.Select(c => c.Destination);

        public override bool CanConnectToValid(IUnitInputPort port)
        {
            return false;
        }

        public override void ConnectToValid(IUnitInputPort port)
        {
            ConnectInvalid(this, port);
        }

        public override void ConnectToInvalid(IUnitInputPort port)
        {
            ConnectInvalid(this, port);
        }

        public override void DisconnectFromValid(IUnitInputPort port)
        {
            DisconnectInvalid(this, port);
        }

        public override void DisconnectFromInvalid(IUnitInputPort port)
        {
            DisconnectInvalid(this, port);
        }

        public override IUnitPort CompatiblePort(IUnit unit)
        {
            return null;
        }
    }
}
