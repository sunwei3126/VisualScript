using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Graph;

namespace VisualScript.Flow
{
    public interface IUnitDebugData : IGraphElementDebugData
    {
        int LastInvokeFrame { get; set; }

        float LastInvokeTime { get; set; }
    }
}
