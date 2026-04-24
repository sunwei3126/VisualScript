using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Graph
{
    public interface IGraphDebugData
    {
        IGraphElementDebugData GetOrCreateElementData(IGraphElementWithDebugData element);

        IGraphDebugData GetOrCreateChildGraphData(IGraphParentElement element);

        IEnumerable<IGraphElementDebugData> ElementsData { get; }
    }
}
