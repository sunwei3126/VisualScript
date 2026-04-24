using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using IoTLogic.Core.Connections;
using IoTLogic.Core.Events;
using IoTLogic.Core.Graph;
using IoTLogic.Core.Groups;
using IoTLogic.Core.Utities;
using IoTLogic.Core.Variables;
using IoTLogic.Flow.Connections;
using IoTLogic.Flow.Framework;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow
{
    [DisplayName("Logic Graph")]
    public sealed class LogicGraph : Graph, IGraphWithVariables, IGraphEventListener
    {
        public LogicGraph()
        {
            LogicNodes = new GraphElementCollection<ILogicNode>(this);
            ControlConnections = new GraphConnectionCollection<ControlConnection, ControlOutput, ControlInput>(this);
            ValueConnections = new GraphConnectionCollection<ValueConnection, ValueOutput, ValueInput>(this);
            InvalidConnections = new GraphConnectionCollection<InvalidConnection, IUnitOutputPort, IUnitInputPort>(this);
            Groups = new GraphElementCollection<GraphGroup>(this);

            Elements.Include(LogicNodes);
            Elements.Include(ControlConnections);
            Elements.Include(ValueConnections);
            Elements.Include(InvalidConnections);
            Elements.Include(Groups);

            controlInputDefinitions = new UnitPortDefinitionCollection<ControlInputDefinition>();
            controlOutputDefinitions = new UnitPortDefinitionCollection<ControlOutputDefinition>();
            valueInputDefinitions = new UnitPortDefinitionCollection<ValueInputDefinition>();
            valueOutputDefinitions = new UnitPortDefinitionCollection<ValueOutputDefinition>();

            Variables = new VariableDeclarations();
        }

        public override IGraphData CreateData()
        {
            return new LogicGraphData(this);
        }

        public void StartListening(GraphStack stack)
        {
            stack.GetGraphData<LogicGraphData>().IsListening = true;
            foreach (var node in LogicNodes)
                (node as IGraphEventListener)?.StartListening(stack);
        }

        public void StopListening(GraphStack stack)
        {
            foreach (var node in LogicNodes)
                (node as IGraphEventListener)?.StopListening(stack);
            stack.GetGraphData<LogicGraphData>().IsListening = false;
        }

        public bool IsListening(GraphStack stack)
        {
            return stack.GetGraphData<LogicGraphData>().IsListening;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetGraphData<LogicGraphData>().IsListening;
        }

        public GraphElementCollection<ILogicNode> LogicNodes { get; }
        public GraphConnectionCollection<ControlConnection, ControlOutput, ControlInput> ControlConnections { get; }
        public GraphConnectionCollection<ValueConnection, ValueOutput, ValueInput> ValueConnections { get; }
        public GraphConnectionCollection<InvalidConnection, IUnitOutputPort, IUnitInputPort> InvalidConnections { get; }
        public GraphElementCollection<GraphGroup> Groups { get; }

        public event Action OnPortDefinitionsChanged;

        private UnitPortDefinitionCollection<ControlInputDefinition> controlInputDefinitions;
        private UnitPortDefinitionCollection<ControlOutputDefinition> controlOutputDefinitions;
        private UnitPortDefinitionCollection<ValueInputDefinition> valueInputDefinitions;
        private UnitPortDefinitionCollection<ValueOutputDefinition> valueOutputDefinitions;

        public UnitPortDefinitionCollection<ControlInputDefinition> ControlInputDefinitions => controlInputDefinitions;
        public UnitPortDefinitionCollection<ControlOutputDefinition> ControlOutputDefinitions => controlOutputDefinitions;
        public UnitPortDefinitionCollection<ValueInputDefinition> ValueInputDefinitions => valueInputDefinitions;
        public UnitPortDefinitionCollection<ValueOutputDefinition> ValueOutputDefinitions => valueOutputDefinitions;

        public IEnumerable<IUnitPortDefinition> PortDefinitions =>
            controlInputDefinitions.Cast<IUnitPortDefinition>()
            .Concat(controlOutputDefinitions)
            .Concat(valueInputDefinitions)
            .Concat(valueOutputDefinitions);

        public IEnumerable<IUnitPortDefinition> ValidPortDefinitions =>
            PortDefinitions.Where(d => d.IsValid);

        public void PortDefinitionsChanged()
        {
            OnPortDefinitionsChanged?.Invoke();
        }

        public VariableDeclarations Variables { get; }

        public IEnumerable<string> GetDynamicVariableNames(VariableKind kind, GraphReference reference)
        {
            if (kind == Variables.Kind)
            {
                foreach (var declaration in Variables)
                    yield return declaration.Name;
            }
        }

        public static LogicGraph WithInputOutput()
        {
            var graph = new LogicGraph();
            graph.LogicNodes.Add(new GraphInput());
            graph.LogicNodes.Add(new GraphOutput());
            return graph;
        }
    }
}