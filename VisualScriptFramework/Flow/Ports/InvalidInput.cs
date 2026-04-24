using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Flow.Connections;

namespace IoTLogic.Flow.Ports
{
    public sealed class InvalidInput : UnitPort<IUnitOutputPort, IUnitOutputPort, InvalidConnection>, IUnitInvalidPort, IUnitInputPort
    {
        public InvalidInput(string key) : base(key) { }

        public override IEnumerable<InvalidConnection> ValidConnections => LogicNode?.Graph?.InvalidConnections.WithDestination(this) ?? Enumerable.Empty<InvalidConnection>();

        public override IEnumerable<InvalidConnection> InvalidConnections => Enumerable.Empty<InvalidConnection>();

        public override IEnumerable<IUnitOutputPort> ValidConnectedPorts =>  ValidConnections.Select(c => c.Source);

        public override IEnumerable<IUnitOutputPort> InvalidConnectedPorts => InvalidConnections.Select(c => c.Source);

        public override bool CanConnectToValid(IUnitOutputPort port)
        {
            return false;
        }

        public override void ConnectToValid(IUnitOutputPort port)
        {
            ConnectInvalid(port, this);
        }

        public override void ConnectToInvalid(IUnitOutputPort port)
        {
            ConnectInvalid(port, this);
        }

        public override void DisconnectFromValid(IUnitOutputPort port)
        {
            DisconnectInvalid(port, this);
        }

        public override void DisconnectFromInvalid(IUnitOutputPort port)
        {
            DisconnectInvalid(port, this);
        }

        public override IUnitPort CompatiblePort(ILogicNode LogicNode)
        {
            return null;
        }
    }
}
