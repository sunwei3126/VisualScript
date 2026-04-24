using System.Linq;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework
{
    /// <summary>
    /// Passes output values from this graph to the parent super LogicNode.
    /// </summary>
    //[UnitCategory("Nesting")]
    //[UnitOrder(2)]
    //[UnitTitle("Output")]
    public sealed class GraphOutput : LogicNode
    {
        public override bool CanDefine => Graph != null;

        protected override void Definition()
        {
            IsControlRoot = true;

            foreach (var controlOutputDefinition in Graph.ValidPortDefinitions.OfType<ControlOutputDefinition>())
            {
                var key = controlOutputDefinition.Key;

                ControlInput(key, (flow) =>
                {
                    var superUnit = flow.stack.GetParent<SubgraphLogicNode>();

                    flow.stack.ExitParentElement();

                    superUnit.EnsureDefined();

                    return superUnit.ControlOutputs[key];
                });
            }

            foreach (var valueOutputDefinition in Graph.ValidPortDefinitions.OfType<ValueOutputDefinition>())
            {
                var key = valueOutputDefinition.Key;
                var type = valueOutputDefinition.Type;

                ValueInput(type, key);
            }
        }

        protected override void AfterDefine()
        {
            Graph.OnPortDefinitionsChanged += Define;
        }

        protected override void BeforeUndefine()
        {
            Graph.OnPortDefinitionsChanged -= Define;
        }
    }
}
