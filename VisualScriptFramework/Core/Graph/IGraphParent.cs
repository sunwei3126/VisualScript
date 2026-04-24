using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Graph
{
    public interface IGraphParent
    {
        IGraph ChildGraph { get; }

        bool IsSerializationRoot { get; }

        IGraph DefaultGraph();
    }
}
