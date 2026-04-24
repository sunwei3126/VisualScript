using System;
using System.ComponentModel;
using IoTLogic.Core.EditorBinding;
using IoTLogic.Core.Events;
using IoTLogic.Core.Graph;
using IoTLogic.Core.Macros;
using IoTLogic.Flow.Framework;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow
{
    //[TypeIcon(typeof(LogicGraph))]
   // [UnitCategory("Nesting")]
    //[UnitTitle("Subgraph")]
    [RenamedFrom("Bolt.SuperUnit")]
    [DisplayName("Subgraph Node")]
    public sealed class SubgraphLogicNode : NesterLogicNode<LogicGraph, Macro<LogicGraph>>, IGraphEventListener, IGraphElementWithData
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }

        public SubgraphLogicNode() : base() { }

        public SubgraphLogicNode(Macro<LogicGraph> macro) : base(macro) { }

        public static SubgraphLogicNode WithInputOutput()
        {
            var superUnit = new SubgraphLogicNode();
            superUnit.nest.Source = GraphSource.Embed;
            superUnit.nest.Embed = LogicGraph.WithInputOutput();
            return superUnit;
        }

        public static SubgraphLogicNode WithStartUpdate()
        {
            var superUnit = new SubgraphLogicNode();
            superUnit.nest.Source = GraphSource.Embed;
           // superUnit.nest.embed = LogicGraph.WithStartUpdate();
            return superUnit;
        }

        public override LogicGraph DefaultGraph()
        {
            return LogicGraph.WithInputOutput();
        }

        protected override void Definition()
        {
            IsControlRoot = true; // TODO: Infer relations instead

            // Using portDefinitions and type checks instead of specific definition collections
            // to avoid duplicates. Iterating only once for speed.

            foreach (var definition in nest.Graph.ValidPortDefinitions)
            {
                if (definition is ControlInputDefinition)
                {
                    var controlInputDefinition = (ControlInputDefinition)definition;
                    var key = controlInputDefinition.Key;

                    ControlInput(key, (flow) =>
                    {
                        foreach (var LogicNode in nest.Graph.LogicNodes)
                        {
                            if (LogicNode is GraphInput)
                            {
                                var inputUnit = (GraphInput)LogicNode;

                                flow.stack.EnterParentElement(this);

                                return inputUnit.ControlOutputs[key];
                            }
                        }

                        return null;
                    });
                }
                else if (definition is ValueInputDefinition)
                {
                    var valueInputDefinition = (ValueInputDefinition)definition;
                    var key = valueInputDefinition.Key;
                    var type = valueInputDefinition.Type;
                    var hasDefaultValue = valueInputDefinition.hasDefaultValue;
                    var defaultValue = valueInputDefinition.defaultValue;

                    var port = ValueInput(type, key);

                    if (hasDefaultValue)
                    {
                        port.SetDefaultValue(defaultValue);
                    }
                }
                else if (definition is ControlOutputDefinition)
                {
                    var controlOutputDefinition = (ControlOutputDefinition)definition;
                    var key = controlOutputDefinition.Key;

                    ControlOutput(key);
                }
                else if (definition is ValueOutputDefinition)
                {
                    var valueOutputDefinition = (ValueOutputDefinition)definition;
                    var key = valueOutputDefinition.Key;
                    var type = valueOutputDefinition.Type;

                    ValueOutput(type, key, (flow) =>
                    {
                        flow.stack.EnterParentElement(this);

                        // Manual looping to avoid LINQ allocation
                        // Also removing check for multiple output nodes for speed
                        // (The first output node will be used without any error)

                        foreach (var LogicNode in nest.Graph.LogicNodes)
                        {
                            if (LogicNode is GraphOutput)
                            {
                                var outputUnit = (GraphOutput)LogicNode;

                                var value = flow.GetValue(outputUnit.ValueInputs[key]);

                                flow.stack.ExitParentElement();

                                return value;
                            }
                        }

                        flow.stack.ExitParentElement();

                        throw new InvalidOperationException("Missing output node when to get value.");
                    });
                }
            }
        }

        public void StartListening(GraphStack stack)
        {
            if (stack.TryEnterParentElement(this))
            {
                nest.Graph.StartListening(stack);
                stack.ExitParentElement();
            }

            stack.GetElementData<Data>(this).isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            stack.GetElementData<Data>(this).isListening = false;

            if (stack.TryEnterParentElement(this))
            {
                nest.Graph.StopListening(stack);
                stack.ExitParentElement();
            }
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetElementData<Data>(this).isListening;
        }

        #region Editing

        public override void AfterAdd()
        {
            base.AfterAdd();

            nest.beforeGraphChange += StopWatchingPortDefinitions;
            nest.afterGraphChange += StartWatchingPortDefinitions;

            StartWatchingPortDefinitions();
        }

        public override void BeforeRemove()
        {
            base.BeforeRemove();

            StopWatchingPortDefinitions();

            nest.beforeGraphChange -= StopWatchingPortDefinitions;
            nest.afterGraphChange -= StartWatchingPortDefinitions;
        }

        private void StopWatchingPortDefinitions()
        {
            if (nest.Graph != null)
            {
                nest.Graph.OnPortDefinitionsChanged -= Define;
            }
        }

        private void StartWatchingPortDefinitions()
        {
            if (nest.Graph != null)
            {
                nest.Graph.OnPortDefinitionsChanged += Define;
            }
        }

        #endregion
    }
}
