using IoTLogic.Core.Graph;

namespace IoTLogic.Core.Events
{
    public interface IGraphEventListenerData : IGraphData
    {
        bool IsListening { get; }
    }
}
