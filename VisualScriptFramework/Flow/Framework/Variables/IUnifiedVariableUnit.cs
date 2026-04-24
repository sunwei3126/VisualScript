using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Variables;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow.Framework
{
    public interface IUnifiedVariableUnit : IUnit
    {
        VariableKind Kind { get; }
        ValueInput Name { get; }
    }
}
