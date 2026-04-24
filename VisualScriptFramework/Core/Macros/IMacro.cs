using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Graph;

namespace VisualScript.Core.Macros
{
    public interface IMacro : IGraphRoot
    {
        IGraph Graph { get; set; }
    }
}
