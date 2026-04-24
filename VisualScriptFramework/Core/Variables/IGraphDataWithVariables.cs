using IoTLogic.Core.Graph;

namespace IoTLogic.Core.Variables
{
    public interface IGraphDataWithVariables : IGraphData
    {
        VariableDeclarations Variables { get; }
    }
}
