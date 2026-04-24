using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Graph;
using VisualScript.Flow.Connections;

namespace VisualScript.Flow.Ports
{
    public interface IUnitPort : IGraphItem
    {
        IUnit Unit { get; set; }

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

        IUnitPort CompatiblePort(IUnit unit);
    }
}
