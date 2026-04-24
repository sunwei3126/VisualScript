using VisualScript.Core.Graph;

namespace VisualScript.Core.Events
{
    public interface IGraphEventListenerData : IGraphData
    {
        bool IsListening { get; }
    }
}
