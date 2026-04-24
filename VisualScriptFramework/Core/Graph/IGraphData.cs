using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Graph
{
    public interface IGraphData
    {
        bool TryGetElementData(IGraphElementWithData element, out IGraphElementData data);

        bool TryGetChildGraphData(IGraphParentElement element, out IGraphData data);

        IGraphElementData CreateElementData(IGraphElementWithData element);

        void FreeElementData(IGraphElementWithData element);

        IGraphData CreateChildGraphData(IGraphParentElement element);

        void FreeChildGraphData(IGraphParentElement element);
    }
}
