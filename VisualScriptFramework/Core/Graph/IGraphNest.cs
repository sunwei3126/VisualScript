using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Macros;

namespace VisualScript.Core.Graph
{
    public interface IGraphNest 
    {
        IGraphNester Nester { get; set; }

        GraphSource Source { get; set; }

        IGraph Embed { get; set; }
        IMacro Macro { get; set; }
        IGraph Graph { get; }

        Type GraphType { get; }

        Type MacroType { get; }

        bool HasBackgroundEmbed { get; }
    }
}
