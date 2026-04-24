using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Flow;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework
{
    public interface IBranchUnit:ILogicNode
    {
        ControlInput enter { get; }
    }
}
