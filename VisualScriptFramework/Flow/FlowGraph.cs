using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VisualScript.Core.Connections;
using VisualScript.Core.Events;
using VisualScript.Core.Graph;
using VisualScript.Core.Groups;
using VisualScript.Core.Utities;
using VisualScript.Core.Variables;
using VisualScript.Flow.Connections;
using VisualScript.Flow.Framework;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow
{
    //[SerializationVersion("A")]
    [DisplayName("Script Graph")]
    public sealed class FlowGraph : Graph, IGraphWithVariables, IGraphEventListener
    {
        public FlowGraph()
        {
            Units = new GraphElementCollection<IUnit>(this);
            ControlConnections = new GraphConnectionCollection<ControlConnection, ControlOutput, ControlInput>(this);
            ValueConnections = new GraphConnectionCollection<ValueConnection, ValueOutput, ValueInput>(this);
            InvalidConnections = new GraphConnectionCollection<InvalidConnection, IUnitOutputPort, IUnitInputPort>(this);
            Groups = new GraphElementCollection<GraphGroup>(this);

            Elements.Include(Units);
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
            return new FlowGraphData(this);
        }

        public void StartListening(GraphStack stack)
        {
            stack.GetGraphData<FlowGraphData>().IsListening = true;

            foreach (var unit in Units)
            {
                (unit as IGraphEventListener)?.StartListening(stack);
            }
        }

        public void StopListening(GraphStack stack)
        {
            foreach (var unit in Units)
            {
                (unit as IGraphEventListener)?.StopListening(stack);
            }

            stack.GetGraphData<FlowGraphData>().IsListening = false;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetGraphData<FlowGraphData>().IsListening;
        }

        #region Variables

        //[Serialize]
        public VariableDeclarations Variables { get; private set; }

        public IEnumerable<string> GetDynamicVariableNames(VariableKind kind, GraphReference reference)
        {
            return Units.OfType<IUnifiedVariableUnit>()
                .Where(v => v.Kind == kind && Flow.CanPredict(v.Name, reference))
                .Select(v => Flow.Predict<string>(v.Name, reference))
                .Where(name => !StringUtility.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name);
        }

        #endregion


        #region Elements

        //[DoNotSerialize]
        public GraphElementCollection<IUnit> Units { get; private set; }

        //[DoNotSerialize]
        public GraphConnectionCollection<ControlConnection, ControlOutput, ControlInput> ControlConnections { get; private set; }

       // [DoNotSerialize]
        public GraphConnectionCollection<ValueConnection, ValueOutput, ValueInput> ValueConnections { get; private set; }

        //[DoNotSerialize]
        public GraphConnectionCollection<InvalidConnection, IUnitOutputPort, IUnitInputPort> InvalidConnections { get; private set; }

        //[DoNotSerialize]
        public GraphElementCollection<GraphGroup> Groups { get; private set; }

        #endregion


        #region Definition

        private const string DefinitionRemoveWarningTitle = "Remove Port Definition";

        private const string DefinitionRemoveWarningMessage = "Removing this definition will break any existing connection to this port. Are you sure you want to continue?";

       // [Serialize]
        //[InspectorLabel("Trigger Inputs")]
       // [InspectorWide(true)]
       //[WarnBeforeRemoving(DefinitionRemoveWarningTitle, DefinitionRemoveWarningMessage)]
        public UnitPortDefinitionCollection<ControlInputDefinition> controlInputDefinitions { get; private set; }

        //[Serialize]
       // [InspectorLabel("Trigger Outputs")]
        //[InspectorWide(true)]
        //[WarnBeforeRemoving(DefinitionRemoveWarningTitle, DefinitionRemoveWarningMessage)]
        public UnitPortDefinitionCollection<ControlOutputDefinition> controlOutputDefinitions { get; private set; }

       // [Serialize]
        //[InspectorLabel("Data Inputs")]
        //[InspectorWide(true)]
       // [WarnBeforeRemoving(DefinitionRemoveWarningTitle, DefinitionRemoveWarningMessage)]
        public UnitPortDefinitionCollection<ValueInputDefinition> valueInputDefinitions { get; private set; }

        //[Serialize]
        //[InspectorLabel("Data Outputs")]
        //[InspectorWide(true)]
        //[WarnBeforeRemoving(DefinitionRemoveWarningTitle, DefinitionRemoveWarningMessage)]
        public UnitPortDefinitionCollection<ValueOutputDefinition> valueOutputDefinitions { get; private set; }

        public IEnumerable<IUnitPortDefinition> ValidPortDefinitions =>
            LinqUtility.Concat<IUnitPortDefinition>(controlInputDefinitions,
                controlOutputDefinitions,
                valueInputDefinitions,
                valueOutputDefinitions)
                .Where(upd => upd.IsValid)
                .DistinctBy(upd => upd.Key);

        public event Action OnPortDefinitionsChanged;

        public void PortDefinitionsChanged()
        {
            OnPortDefinitionsChanged?.Invoke();
        }

        #endregion

        public static FlowGraph WithInputOutput()
        {
            return new FlowGraph()
            {
                Units =
                {
                    new GraphInput() { },
                    new GraphOutput() {}
                }
            };
        }
    }
}


