using System.Linq;
using VisualScript.Core.EditorBinding;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow.Framework
{
    /// <summary>
    /// Fetches input values from the parent super unit for this graph.
    /// </summary>
    //[UnitCategory("Nesting")]
    //[UnitOrder(1)]
    //[UnitTitle("Input")]
    public sealed class GraphInput : Unit
    {
        public override bool CanDefine => Graph != null;

        protected override void Definition()
        {
            IsControlRoot = true;

            foreach (var controlInputDefinition in Graph.ValidPortDefinitions.OfType<ControlInputDefinition>())
            {
                ControlOutput(controlInputDefinition.Key);
            }

            foreach (var valueInputDefinition in Graph.ValidPortDefinitions.OfType<ValueInputDefinition>())
            {
                var key = valueInputDefinition.Key;
                var type = valueInputDefinition.Type;

                ValueOutput(type, key, (flow) =>
                {
                    var superUnit = flow.stack.GetParent<SubgraphUnit>();

                    if (flow.enableDebug)
                    {
                        var editorData = flow.stack.GetElementDebugData<IUnitDebugData>(superUnit);

                        editorData.LastInvokeFrame = EditorTimeBinding.frame;
                        editorData.LastInvokeTime = EditorTimeBinding.time;
                    }

                    flow.stack.ExitParentElement();
                    superUnit.EnsureDefined();
                    var value = flow.GetValue(superUnit.ValueInputs[key], type);
                    flow.stack.EnterParentElement(superUnit);

                    return value;
                });
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
