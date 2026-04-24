using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Core.Graph;

namespace IoTLogic.Core.Macros
{
    public interface IMacro : IGraphRoot
    {
        IGraph Graph { get; set; }
    }
}
