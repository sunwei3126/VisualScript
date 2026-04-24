using System;
using IoTLogic.Core.Reflection;

namespace IoTLogic.Core.Graph
{
    public interface IGraph : IDisposable, IPrewarmable
    {
        MergedGraphElementCollection Elements { get; }

        string Title { get; }

        string Summary { get; }

        IGraphData CreateData();

        IGraphDebugData CreateDebugData();

        void Instantiate(GraphReference instance);

        void Uninstantiate(GraphReference instance);
    }
}
