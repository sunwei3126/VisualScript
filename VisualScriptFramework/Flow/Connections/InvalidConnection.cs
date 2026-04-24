using System;
using System.Linq;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow.Connections
{
    public sealed class InvalidConnection : UnitConnection<IUnitOutputPort, IUnitInputPort>, IUnitConnection
    {
        [Obsolete("ConstructorWarning")]
        public InvalidConnection() : base() { }

        public InvalidConnection(IUnitOutputPort source, IUnitInputPort destination) : base(source, destination) { }

        public override void AfterRemove()
        {
            base.AfterRemove();
            Source.Unit.RemoveUnconnectedInvalidPorts();
            Destination.Unit.RemoveUnconnectedInvalidPorts();
        }

        #region Ports

        public override IUnitOutputPort Source => sourceUnit.Outputs.Single(p => p.Key == sourceKey);

        public override IUnitInputPort Destination => destinationUnit.Inputs.Single(p => p.Key == destinationKey);

        public IUnitOutputPort ValidSource => sourceUnit.ValidOutputs.Single(p => p.Key == sourceKey);

        public IUnitInputPort ValidDestination => destinationUnit.ValidInputs.Single(p => p.Key == destinationKey);

        #endregion

        #region Dependencies

        public override bool SourceExists => sourceUnit.Outputs.Any(p => p.Key == sourceKey);

        public override bool DestinationExists => destinationUnit.Inputs.Any(p => p.Key == destinationKey);

        public bool validSourceExists => sourceUnit.ValidOutputs.Any(p => p.Key == sourceKey);

        public bool validDestinationExists => destinationUnit.ValidInputs.Any(p => p.Key == destinationKey);

        public override bool HandleDependencies()
        {
            // Replace the invalid connection with a valid connection if it can be created instead.
            if (validSourceExists && validDestinationExists && ValidSource.CanValidlyConnectTo(ValidDestination))
            {
                ValidSource.ValidlyConnectTo(ValidDestination);

                return false;
            }

            // Add the invalid ports to the nodes if need be
            if (!SourceExists)
            {
                sourceUnit.InvalidOutputs.Add(new InvalidOutput(sourceKey));
            }

            if (!DestinationExists)
            {
                destinationUnit.InvalidInputs.Add(new InvalidInput(destinationKey));
            }

            return true;
        }

        #endregion
    }
}
