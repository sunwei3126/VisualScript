using VisualScript.Core.Events;
using VisualScript.Core.Graph;
using VisualScript.Core.Reflection;
using VisualScript.Core.Variables;

namespace VisualScript.Flow
{
    public sealed class FlowGraphData : GraphData<FlowGraph>, IGraphDataWithVariables, IGraphEventListenerData
    {
        public VariableDeclarations Variables { get; }

        public bool IsListening { get; set; }

        public FlowGraphData(FlowGraph definition) : base(definition)
        {
            Variables = definition.Variables.CloneViaFakeSerialization();
        }
    }
}
