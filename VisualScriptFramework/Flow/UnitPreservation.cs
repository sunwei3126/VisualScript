using System;
using System.Collections.Generic;
using System.Linq;
using VisualScript.Core.Pooling;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow
{
    public sealed class UnitPreservation : IPoolable
    {
        private struct UnitPortPreservation
        {
            public readonly IUnit Unit;

            public readonly string Key;

            public UnitPortPreservation(IUnitPort port)
            {
                Unit = port.Unit;
                Key = port.Key;
            }

            public UnitPortPreservation(IUnit unit, string key)
            {
                this.Unit = unit;
                this.Key = key;
            }

            public IUnitPort GetOrCreateInput(out InvalidInput newInvalidInput)
            {
                var key = this.Key;

                if (!Unit.Inputs.Any(p => p.Key == key))
                {
                    newInvalidInput = new InvalidInput(key);
                    Unit.InvalidInputs.Add(newInvalidInput);
                }
                else
                {
                    newInvalidInput = null;
                }

                return Unit.Inputs.Single(p => p.Key == key);
            }

            public IUnitPort GetOrCreateOutput(out InvalidOutput newInvalidOutput)
            {
                var key = this.Key;

                if (!Unit.Outputs.Any(p => p.Key == key))
                {
                    newInvalidOutput = new InvalidOutput(key);
                    Unit.InvalidOutputs.Add(newInvalidOutput);
                }
                else
                {
                    newInvalidOutput = null;
                }

                return Unit.Outputs.Single(p => p.Key == key);
            }
        }

        private readonly Dictionary<string, object> DefaultValues = new Dictionary<string, object>();

        private readonly Dictionary<string, List<UnitPortPreservation>> InputConnections = new Dictionary<string, List<UnitPortPreservation>>();

        private readonly Dictionary<string, List<UnitPortPreservation>> OutputConnections = new Dictionary<string, List<UnitPortPreservation>>();

        private bool disposed;

        void IPoolable.New()
        {
            disposed = false;
        }

        void IPoolable.Free()
        {
            disposed = true;

            foreach (var inputConnection in InputConnections)
            {
                ListPool<UnitPortPreservation>.Free(inputConnection.Value);
            }

            foreach (var outputConnection in OutputConnections)
            {
                ListPool<UnitPortPreservation>.Free(outputConnection.Value);
            }

            DefaultValues.Clear();
            InputConnections.Clear();
            OutputConnections.Clear();
        }

        private UnitPreservation() { }

        public static UnitPreservation Preserve(IUnit unit)
        {
            var preservation = GenericPool<UnitPreservation>.New(() => new UnitPreservation());

            foreach (var defaultValue in unit.DefaultValues)
            {
                preservation.DefaultValues.Add(defaultValue.Key, defaultValue.Value);
            }

            foreach (var input in unit.Inputs)
            {
                if (input.HasAnyConnection)
                {
                    preservation.InputConnections.Add(input.Key, ListPool<UnitPortPreservation>.New());

                    foreach (var connectedPort in input.ConnectedPorts)
                    {
                        preservation.InputConnections[input.Key].Add(new UnitPortPreservation(connectedPort));
                    }
                }
            }

            foreach (var output in unit.Outputs)
            {
                if (output.HasAnyConnection)
                {
                    preservation.OutputConnections.Add(output.Key, ListPool<UnitPortPreservation>.New());

                    foreach (var connectedPort in output.ConnectedPorts)
                    {
                        preservation.OutputConnections[output.Key].Add(new UnitPortPreservation(connectedPort));
                    }
                }
            }

            return preservation;
        }

        public void RestoreTo(IUnit unit)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(ToString());
            }

            // Restore inline values if possible

            foreach (var previousDefaultValue in DefaultValues)
            {
                if (unit.DefaultValues.ContainsKey(previousDefaultValue.Key) &&
                    unit.ValueInputs.Contains(previousDefaultValue.Key) &&
                    unit.ValueInputs[previousDefaultValue.Key].Type.IsAssignableFrom(previousDefaultValue.Value.GetType()))
                {
                    unit.DefaultValues[previousDefaultValue.Key] = previousDefaultValue.Value;
                }
            }

            // Restore connections if possible

            foreach (var previousInputConnections in InputConnections)
            {
                var previousInputPort = new UnitPortPreservation(unit, previousInputConnections.Key);
                var previousOutputPorts = previousInputConnections.Value;

                foreach (var previousOutputPort in previousOutputPorts)
                {
                    RestoreConnection(previousOutputPort, previousInputPort);
                }
            }

            foreach (var previousOutputConnections in OutputConnections)
            {
                var previousOutputPort = new UnitPortPreservation(unit, previousOutputConnections.Key);
                var previousInputPorts = previousOutputConnections.Value;

                foreach (var previousInputPort in previousInputPorts)
                {
                    RestoreConnection(previousOutputPort, previousInputPort);
                }
            }

            GenericPool<UnitPreservation>.Free(this);
        }

        private void RestoreConnection(UnitPortPreservation sourcePreservation, UnitPortPreservation destinationPreservation)
        {
            InvalidOutput newInvalidSource;
            InvalidInput newInvalidDestination;

            var source = sourcePreservation.GetOrCreateOutput(out newInvalidSource);
            var destination = destinationPreservation.GetOrCreateInput(out newInvalidDestination);

            if (source.CanValidlyConnectTo(destination))
            {
                source.ValidlyConnectTo(destination);
            }
            else if (source.CanInvalidlyConnectTo(destination))
            {
                source.InvalidlyConnectTo(destination);
            }
            else
            {
                // In this case, we created invalid ports to attempt a connection,
                // but even that failed (due to, for example, a cross-graph restoration).
                // Therefore, we need to delete the invalid ports we created.

                if (newInvalidSource != null)
                {
                    sourcePreservation.Unit.InvalidOutputs.Remove(newInvalidSource);
                }

                if (newInvalidDestination != null)
                {
                    destinationPreservation.Unit.InvalidInputs.Remove(newInvalidDestination);
                }
            }
        }
    }
}
