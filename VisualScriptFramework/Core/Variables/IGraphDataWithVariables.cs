using VisualScript.Core.Graph;

namespace VisualScript.Core.Variables
{
    public interface IGraphDataWithVariables : IGraphData
    {
        VariableDeclarations Variables { get; }
    }
}
