using IoTLogic.Core.Graph;

namespace IoTLogic.Flow.Connections
{
    public interface IUnitConnectionDebugData : IGraphElementDebugData
    {
        int LastInvokeFrame { get; set; }

        float LastInvokeTime { get; set; }
    }
}
