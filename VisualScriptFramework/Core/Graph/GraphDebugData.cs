using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Graph
{
    public class GraphDebugData : IGraphDebugData
    {
        protected Dictionary<IGraphElementWithDebugData, IGraphElementDebugData> elementsData { get; } = new Dictionary<IGraphElementWithDebugData, IGraphElementDebugData>();

        protected Dictionary<IGraphParentElement, IGraphDebugData> childrenGraphsData { get; } = new Dictionary<IGraphParentElement, IGraphDebugData>();

        IEnumerable<IGraphElementDebugData> IGraphDebugData.ElementsData => elementsData.Values;

        public GraphDebugData(IGraph definition) { }

        public IGraphElementDebugData GetOrCreateElementData(IGraphElementWithDebugData element)
        {
            if (!elementsData.TryGetValue(element, out var elementDebugData))
            {
                elementDebugData = element.CreateDebugData();
                elementsData.Add(element, elementDebugData);
            }

            return elementDebugData;
        }

        public IGraphDebugData GetOrCreateChildGraphData(IGraphParentElement element)
        {
            if (!childrenGraphsData.TryGetValue(element, out var data))
            {
                data = new GraphDebugData(element.ChildGraph);
                childrenGraphsData.Add(element, data);
            }

            return data;
        }
    }
}
