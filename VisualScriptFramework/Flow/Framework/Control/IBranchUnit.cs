using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Flow;
using VisualScript.Flow.Ports;

namespace VisualScriptFramework.Flow.Framework
{
    public interface IBranchUnit:IUnit
    {
        ControlInput enter { get; }
    }
}
