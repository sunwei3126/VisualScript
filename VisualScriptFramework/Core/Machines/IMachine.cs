using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Graph;

namespace VisualScript.Core.Machines
{
    public interface IMachine : IGraphRoot, IGraphNester
    {
        IGraphData GraphData { get; set; }
    }
}
