using System;
using System.Linq;
using VisualScript.Core.Connections;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow.Connections
{
    public sealed class ControlConnection : UnitConnection<ControlOutput, ControlInput>, IUnitConnection
    {
        [Obsolete("ConstructorWarning")]
        public ControlConnection() : base() { }

        public ControlConnection(ControlOutput source, ControlInput destination) : base(source, destination)
        {
            if (source.HasValidConnection)
            {
                throw new InvalidConnectionException("Control output ports do not support multiple connections.");
            }
        }

        #region Ports

        public override ControlOutput Source => sourceUnit.ControlOutputs[sourceKey];

        public override ControlInput Destination => destinationUnit.ControlInputs[destinationKey];

        IUnitOutputPort IConnection<IUnitOutputPort, IUnitInputPort>.Source => Source;

        IUnitInputPort IConnection<IUnitOutputPort, IUnitInputPort>.Destination => Destination;

        #endregion

        #region Dependencies

        public override bool SourceExists => sourceUnit.ControlOutputs.Contains(sourceKey);

        public override bool DestinationExists => destinationUnit.ControlInputs.Contains(destinationKey);

        #endregion
    }
}
