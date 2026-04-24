using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Graph
{
    public interface IGraphItem
    {
        IGraph Graph { get; }
    }
}
