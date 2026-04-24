using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Graph
{
    public interface IGraphElementDebugData
    {
        // Being lazy with the interfaces here to simplify things
        Exception RuntimeException { get; set; }
    }
}
