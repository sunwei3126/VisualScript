using IoTLogic.Core.Events;
using IoTLogic.Core.Graph;
using IoTLogic.Core.Reflection;
using IoTLogic.Core.Variables;

namespace IoTLogic.Flow
{
    public sealed class LogicGraphData : GraphData<LogicGraph>, IGraphDataWithVariables, IGraphEventListenerData
    {
        public VariableDeclarations Variables { get; }

        public bool IsListening { get; set; }

        public LogicGraphData(LogicGraph definition) : base(definition)
        {
            Variables = definition.Variables.CloneViaFakeSerialization();
        }
    }
}
