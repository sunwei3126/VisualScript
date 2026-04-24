using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Core.Graph;

namespace IoTLogic.Core.Machines
{
    public interface IMachine : IGraphRoot, IGraphNester
    {
        IGraphData GraphData { get; set; }
    }
}
