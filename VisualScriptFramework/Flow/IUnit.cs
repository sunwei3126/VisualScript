using System;
using System.Collections.Generic;
using System.Linq;
using VisualScript.Core.Connections;
using VisualScript.Core.Ensure;
using VisualScript.Core.Graph;
using VisualScript.Core.Reflection;
using VisualScript.Flow.Connections;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow
{
    public interface IUnit : IGraphElementWithDebugData
    {
        new FlowGraph Graph { get; }

        #region Definition

        bool CanDefine { get; }

        bool IsDefined { get; }

        bool FailedToDefine { get; }

        Exception DefinitionException { get; }

        void Define();

        void EnsureDefined();

        void RemoveUnconnectedInvalidPorts();

        #endregion

        #region Default Values

        Dictionary<string, object> DefaultValues { get; }

        #endregion

        #region Ports

        IUnitPortCollection<ControlInput> ControlInputs { get; }

        IUnitPortCollection<ControlOutput> ControlOutputs { get; }

        IUnitPortCollection<ValueInput> ValueInputs { get; }

        IUnitPortCollection<ValueOutput> ValueOutputs { get; }

        IUnitPortCollection<InvalidInput> InvalidInputs { get; }

        IUnitPortCollection<InvalidOutput> InvalidOutputs { get; }

        IEnumerable<IUnitInputPort> Inputs { get; }

        IEnumerable<IUnitOutputPort> Outputs { get; }

        IEnumerable<IUnitInputPort> ValidInputs { get; }

        IEnumerable<IUnitOutputPort> ValidOutputs { get; }

        IEnumerable<IUnitPort> Ports { get; }

        IEnumerable<IUnitPort> InvalidPorts { get; }

        IEnumerable<IUnitPort> ValidPorts { get; }

        void PortsChanged();

        event Action OnPortsChanged;

        #endregion

        #region Connections

        IConnectionCollection<IUnitRelation, IUnitPort, IUnitPort> Relations { get; }

        IEnumerable<IUnitConnection> Connections { get; }

        #endregion

        #region Analysis

        bool IsControlRoot { get; }

        #endregion

    }
        public static class XUnit
        {
            public static ValueInput CompatibleValueInput(this IUnit unit, Type outputType)
            {
                Ensure.That(nameof(outputType)).IsNotNull(outputType);

                return unit.ValueInputs
                    .Where(valueInput => ConversionUtility.CanConvert(outputType, valueInput.Type, false))
                    .OrderBy((valueInput) =>
                    {
                        var exactType = outputType == valueInput.Type;
                        var free = !valueInput.HasValidConnection;

                        if (free && exactType)
                        {
                            return 1;
                        }
                        else if (free)
                        {
                            return 2;
                        }
                        else if (exactType)
                        {
                            return 3;
                        }
                        else
                        {
                            return 4;
                        }
                    }).FirstOrDefault();
            }

            public static ValueOutput CompatibleValueOutput(this IUnit unit, Type inputType)
            {
                Ensure.That(nameof(inputType)).IsNotNull(inputType);

                return unit.ValueOutputs
                    .Where(valueOutput => ConversionUtility.CanConvert(valueOutput.Type, inputType, false))
                    .OrderBy((valueOutput) =>
                    {
                        var exactType = inputType == valueOutput.Type;
                        var free = !valueOutput.HasValidConnection;

                        if (free && exactType)
                        {
                            return 1;
                        }
                        else if (free)
                        {
                            return 2;
                        }
                        else if (exactType)
                        {
                            return 3;
                        }
                        else
                        {
                            return 4;
                        }
                    }).FirstOrDefault();
            }
        }
    
}
