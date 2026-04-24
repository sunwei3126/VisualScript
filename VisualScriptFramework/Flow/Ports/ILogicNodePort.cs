using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Core.Graph;
using IoTLogic.Flow.Connections;

namespace IoTLogic.Flow.Ports
{
    public interface IUnitPort : IGraphItem
    {
        ILogicNode LogicNode { get; set; }

        string Key { get; }

        IEnumerable<IUnitRelation> Relations { get; }

        IEnumerable<IUnitConnection> ValidConnections { get; }

        IEnumerable<InvalidConnection> InvalidConnections { get; }

        IEnumerable<IUnitConnection> Connections { get; }

        IEnumerable<IUnitPort> ConnectedPorts { get; }

        bool HasAnyConnection { get; }

        bool HasValidConnection { get; }

        bool HasInvalidConnection { get; }

        bool CanInvalidlyConnectTo(IUnitPort port);

        bool CanValidlyConnectTo(IUnitPort port);

        void InvalidlyConnectTo(IUnitPort port);

        void ValidlyConnectTo(IUnitPort port);

        void Disconnect();

        IUnitPort CompatiblePort(ILogicNode LogicNode);
    }
}
