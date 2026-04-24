using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Core.Graph;

namespace IoTLogic.Flow
{
    public interface IUnitDebugData : IGraphElementDebugData
    {
        int LastInvokeFrame { get; set; }

        float LastInvokeTime { get; set; }
    }
}
