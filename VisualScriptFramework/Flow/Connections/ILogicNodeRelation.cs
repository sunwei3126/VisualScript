using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Core.Connections;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Connections
{
    public interface IUnitRelation : IConnection<IUnitPort, IUnitPort> { }
}
