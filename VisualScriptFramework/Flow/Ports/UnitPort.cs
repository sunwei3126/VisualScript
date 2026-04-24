using System.Collections.Generic;
using System.Linq;
using VisualScript.Core.Connections;
using VisualScript.Core.Ensure;
using VisualScript.Core.Graph;
using VisualScript.Core.Utities;
using VisualScript.Flow.Connections;

namespace VisualScript.Flow.Ports
{
    public abstract class UnitPort<TValidOther, TInvalidOther, TExternalConnection> : IUnitPort
       where TValidOther : IUnitPort
       where TInvalidOther : IUnitPort
       where TExternalConnection : IUnitConnection
    {
        protected UnitPort(string key)
        {
            Ensure.That(nameof(key)).IsNotNull(key);
            this.Key = key;
        }

        public IUnit Unit { get; set; }

        public string Key { get; }

        public IGraph Graph => Unit?.Graph;

        public IEnumerable<IUnitRelation> Relations =>
            LinqUtility.Concat<IUnitRelation>(Unit.Relations.WithSource(this),
                Unit.Relations.WithDestination(this)).Distinct();

        public abstract IEnumerable<TExternalConnection> ValidConnections { get; }

        public abstract IEnumerable<InvalidConnection> InvalidConnections { get; }

        public abstract IEnumerable<TValidOther> ValidConnectedPorts { get; }

        public abstract IEnumerable<TInvalidOther> InvalidConnectedPorts { get; }

        IEnumerable<IUnitConnection> IUnitPort.ValidConnections => ValidConnections.Cast<IUnitConnection>();

        public IEnumerable<IUnitConnection> Connections => LinqUtility.Concat<IUnitConnection>(ValidConnections, InvalidConnections);

        public IEnumerable<IUnitPort> ConnectedPorts => LinqUtility.Concat<IUnitPort>(ValidConnectedPorts, InvalidConnectedPorts);

        public bool HasAnyConnection => HasValidConnection || HasInvalidConnection;

        // Allow for more efficient overrides

        public virtual bool HasValidConnection => ValidConnections.Any();

        public virtual bool HasInvalidConnection => InvalidConnections.Any();

        private bool CanConnectTo(IUnitPort port)
        {
            Ensure.That(nameof(port)).IsNotNull(port);

            return Unit != null && // We belong to a unit
                port.Unit != null &&    // Port belongs to a unit
                port.Unit != Unit &&    // that is different than the current one
                port.Unit.Graph == Unit.Graph;    // but is on the same graph.
        }

        public bool CanValidlyConnectTo(IUnitPort port)
        {
            return CanConnectTo(port) && port is TValidOther && CanConnectToValid((TValidOther)port);
        }

        public bool CanInvalidlyConnectTo(IUnitPort port)
        {
            return CanConnectTo(port) && port is TInvalidOther && CanConnectToInvalid((TInvalidOther)port);
        }

        public void ValidlyConnectTo(IUnitPort port)
        {
            Ensure.That(nameof(port)).IsNotNull(port);

            if (!(port is TValidOther))
            {
                throw new InvalidConnectionException();
            }

            ConnectToValid((TValidOther)port);
        }

        public void InvalidlyConnectTo(IUnitPort port)
        {
            Ensure.That(nameof(port)).IsNotNull(port);

            if (!(port is TInvalidOther))
            {
                throw new InvalidConnectionException();
            }

            ConnectToInvalid((TInvalidOther)port);
        }

        public void Disconnect()
        {
            while (ValidConnectedPorts.Any())
            {
                DisconnectFromValid(ValidConnectedPorts.First());
            }

            while (InvalidConnectedPorts.Any())
            {
                DisconnectFromInvalid(InvalidConnectedPorts.First());
            }
        }

        public abstract bool CanConnectToValid(TValidOther port);

        public bool CanConnectToInvalid(TInvalidOther port)
        {
            return true;
        }

        public abstract void ConnectToValid(TValidOther port);

        public abstract void ConnectToInvalid(TInvalidOther port);

        public abstract void DisconnectFromValid(TValidOther port);

        public abstract void DisconnectFromInvalid(TInvalidOther port);

        public abstract IUnitPort CompatiblePort(IUnit unit);

        protected void ConnectInvalid(IUnitOutputPort source, IUnitInputPort destination)
        {
            var connection = Unit.Graph.InvalidConnections.SingleOrDefault(c => c.Source == source && c.Destination == destination);

            if (connection != null)
            {
                return;
            }

            Unit.Graph.InvalidConnections.Add(new InvalidConnection(source, destination));
        }

        protected void DisconnectInvalid(IUnitOutputPort source, IUnitInputPort destination)
        {
            var connection = Unit.Graph.InvalidConnections.SingleOrDefault(c => c.Source == source && c.Destination == destination);

            if (connection == null)
            {
                return;
            }

            Unit.Graph.InvalidConnections.Remove(connection);
        }
    }
}
