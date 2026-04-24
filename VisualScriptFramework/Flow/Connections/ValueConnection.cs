using System;
using System.Collections.Generic;
using System.Linq;
using VisualScript.Core.Connections;
using VisualScript.Core.Graph;
using VisualScript.Core.Reflection;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow.Connections
{
    public sealed class ValueConnection : UnitConnection<ValueOutput, ValueInput>, IUnitConnection
    {
        public class DebugData : UnitConnectionDebugData
        {
            public object lastValue { get; set; }

            public bool assignedLastValue { get; set; }
        }

        public override IGraphElementDebugData CreateDebugData()
        {
            return new DebugData();
        }

        [Obsolete("ConstructorWarning")]
        public ValueConnection() : base() { }

        public ValueConnection(ValueOutput source, ValueInput destination) : base(source, destination)
        {
            if (destination.HasValidConnection)
            {
                throw new InvalidConnectionException("Value input ports do not support multiple connections.");
            }

            if (!source.Type.IsConvertibleTo(destination.Type, false))
            {
                throw new InvalidConnectionException($"Cannot convert from '{source.Type}' to '{destination.Type}'.");
            }
        }

        #region Ports

        public override ValueOutput Source => sourceUnit.ValueOutputs[sourceKey];

        public override ValueInput Destination => destinationUnit.ValueInputs[destinationKey];

        IUnitOutputPort IConnection<IUnitOutputPort, IUnitInputPort>.Source => Source;

        IUnitInputPort IConnection<IUnitOutputPort, IUnitInputPort>.Destination => Destination;

        #endregion

        #region Dependencies

        public override bool SourceExists => sourceUnit.ValueOutputs.Contains(sourceKey);

        public override bool DestinationExists => destinationUnit.ValueInputs.Contains(destinationKey);

        #endregion
    }
}
