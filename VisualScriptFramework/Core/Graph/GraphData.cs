using System;
using System.Collections.Generic;

namespace IoTLogic.Core.Graph
{
    public class GraphData<TGraph> : IGraphData
        where TGraph : class, IGraph
    {
        public GraphData(TGraph definition)
        {
            this.definition = definition;
        }

        protected TGraph definition { get; }

        protected Dictionary<IGraphElementWithData, IGraphElementData> elementsData { get; } = new Dictionary<IGraphElementWithData, IGraphElementData>();

        protected Dictionary<IGraphParentElement, IGraphData> childrenGraphsData { get; } = new Dictionary<IGraphParentElement, IGraphData>();

        protected Dictionary<Guid, IGraphElementData> phantomElementsData { get; } = new Dictionary<Guid, IGraphElementData>();

        protected Dictionary<Guid, IGraphData> phantomChildrenGraphsData { get; } = new Dictionary<Guid, IGraphData>();

        public bool TryGetElementData(IGraphElementWithData element, out IGraphElementData data)
        {
            return elementsData.TryGetValue(element, out data);
        }

        public bool TryGetChildGraphData(IGraphParentElement element, out IGraphData data)
        {
            return childrenGraphsData.TryGetValue(element, out data);
        }

        public IGraphElementData CreateElementData(IGraphElementWithData element)
        {
            // Debug.Log($"Creating element data for {element}");

            if (elementsData.ContainsKey(element))
            {
                throw new InvalidOperationException($"Graph data already contains element data for {element}.");
            }

            IGraphElementData elementData;

            if (phantomElementsData.TryGetValue(element.Guid, out elementData))
            {
                // Debug.Log($"Restoring phantom element data for {element}.");
                phantomElementsData.Remove(element.Guid);
            }
            else
            {
                elementData = element.CreateData();
            }

            elementsData.Add(element, elementData);

            return elementData;
        }

        public void FreeElementData(IGraphElementWithData element)
        {
            // Debug.Log($"Freeing element data for {element}");

            if (elementsData.TryGetValue(element, out var elementData))
            {
                elementsData.Remove(element);
                phantomElementsData.Add(element.Guid, elementData);
            }
            else
            {
                Console.WriteLine($"Graph data does not contain element data to free for {element}.");
            }
        }

        public IGraphData CreateChildGraphData(IGraphParentElement element)
        {
            // Debug.Log($"Creating child graph data for {element}");

            if (childrenGraphsData.ContainsKey(element))
            {
                throw new InvalidOperationException($"Graph data already contains child graph data for {element}.");
            }

            IGraphData childGraphData;

            if (phantomChildrenGraphsData.TryGetValue(element.Guid, out childGraphData))
            {
                // Debug.Log($"Restoring phantom child graph data for {element}.");
                phantomChildrenGraphsData.Remove(element.Guid);
            }
            else
            {
                childGraphData = element.ChildGraph.CreateData();
            }

            childrenGraphsData.Add(element, childGraphData);

            return childGraphData;
        }

        public void FreeChildGraphData(IGraphParentElement element)
        {
            // Debug.Log($"Freeing child graph data for {element}");

            if (childrenGraphsData.TryGetValue(element, out var childGraphData))
            {
                childrenGraphsData.Remove(element);
                phantomChildrenGraphsData.Add(element.Guid, childGraphData);
            }
            else
            {
                Console.WriteLine($"Graph data does not contain child graph data to free for {element}.");
            }
        }
    }
}
