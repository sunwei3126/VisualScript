using System;
using System.Linq;
using VisualScript.Core.Connections;
using VisualScript.Core.Ensure;
using VisualScript.Core.Graph;
using VisualScript.Core.Utities;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow.Connections
{
    /* Implementation note:
    * Using an abstract base class works as a type unification workaround.
    * https://stackoverflow.com/questions/22721763
    * https://stackoverflow.com/a/7664919
    *
    * However, this forces us to use concrete classes for connections
    * instead of interfaces. In other words, no IControlConnection / IValueConnection.
    * If we did use interfaces, there would be ambiguity that needs to be resolved
    * at every reference to the source or destination.
    *
    * However, using a disambiguator hack seems to confuse even recent Mono runtime versions of Unity
    * and breaks its vtable. Sometimes, method pointers are just plain wrong.
    * I'm guessing this is specifically due to InvalidConnection, which actually
    * does unify the types; what the C# warning warned about.
    * https://stackoverflow.com/q/50051657/154502
    *
    * THEREFORE, IUnitConnection has to be implemented at the concrete class level,
    * because at that point the type unification warning is moot, because the type arguments are
    * provided.
    */

    public abstract class UnitConnection<TSourcePort, TDestinationPort> : GraphElement<FlowGraph>, IConnection<TSourcePort, TDestinationPort>
        where TSourcePort : class, IUnitOutputPort
        where TDestinationPort : class, IUnitInputPort
    {
        [Obsolete("ConstructorWarning")]
        protected UnitConnection() { }

        protected UnitConnection(TSourcePort source, TDestinationPort destination)
        {
            Ensure.That(nameof(source)).IsNotNull(source);
            Ensure.That(nameof(destination)).IsNotNull(destination);

            if (source.Unit.Graph != destination.Unit.Graph)
            {
                throw new NotSupportedException("Cannot create connections across graphs.");
            }

            if (source.Unit == destination.Unit)
            {
                throw new InvalidConnectionException("Cannot create connections on the same unit.");
            }

            sourceUnit = source.Unit;
            sourceKey = source.Key;
            destinationUnit = destination.Unit;
            destinationKey = destination.Key;
        }

        public virtual IGraphElementDebugData CreateDebugData()
        {
            return new UnitConnectionDebugData();
        }

        #region Ports

        //[Serialize]
        protected IUnit sourceUnit { get; private set; }

        //[Serialize]
        protected string sourceKey { get; private set; }

        //[Serialize]
        protected IUnit destinationUnit { get; private set; }

        //[Serialize]
        protected string destinationKey { get; private set; }

        //[DoNotSerialize]
        public abstract TSourcePort Source { get; }

        //[DoNotSerialize]
        public abstract TDestinationPort Destination { get; }

        #endregion

        #region Dependencies

        public override int DependencyOrder => 1;

        public abstract bool SourceExists { get; }

        public abstract bool DestinationExists { get; }

        protected void CopyFrom(UnitConnection<TSourcePort, TDestinationPort> source)
        {
            base.CopyFrom(source);
        }

        public override bool HandleDependencies()
        {
            // Replace the connection with an invalid connection if the ports are either missing or incompatible.
            // If the ports are missing, create invalid ports if required.

            var valid = true;
            IUnitOutputPort source;
            IUnitInputPort destination;

            if (!SourceExists)
            {
                if (!sourceUnit.InvalidOutputs.Contains(sourceKey))
                {
                    sourceUnit.InvalidOutputs.Add(new InvalidOutput(sourceKey));
                }

                source = sourceUnit.InvalidOutputs[sourceKey];
                valid = false;
            }
            else
            {
                source = this.Source;
            }

            if (!DestinationExists)
            {
                if (!destinationUnit.InvalidInputs.Contains(destinationKey))
                {
                    destinationUnit.InvalidInputs.Add(new InvalidInput(destinationKey));
                }

                destination = destinationUnit.InvalidInputs[destinationKey];
                valid = false;
            }
            else
            {
                destination = this.Destination;
            }

            if (!source.CanValidlyConnectTo(destination))
            {
                valid = false;
            }

            if (!valid && source.CanInvalidlyConnectTo(destination))
            {
                source.InvalidlyConnectTo(destination);

                Console.WriteLine($"Could not load connection between '{source.Key}' of '{sourceUnit}' and '{destination.Key}' of '{destinationUnit}'.");
            }

            return valid;
        }

        #endregion

        #region Analytics

        public override AnalyticsIdentifier GetAnalyticsIdentifier()
        {
            return null;
        }

        #endregion
    }
}
