using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Core.Variables;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework
{
    public interface IUnifiedVariableUnit : ILogicNode
    {
        VariableKind Kind { get; }
        ValueInput Name { get; }
    }
}
