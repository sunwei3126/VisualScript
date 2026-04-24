using VisualScript.Core.Graph;

namespace VisualScript.Flow.Connections
{
    public interface IUnitConnectionDebugData : IGraphElementDebugData
    {
        int LastInvokeFrame { get; set; }

        float LastInvokeTime { get; set; }
    }
}
