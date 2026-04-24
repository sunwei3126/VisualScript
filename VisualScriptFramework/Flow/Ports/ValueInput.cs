using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using VisualScript.Core.Ensure;
using VisualScript.Core.Reflection;
using VisualScript.Flow.Connections;

namespace VisualScript.Flow.Ports
{
    public sealed class ValueInput : UnitPort<ValueOutput, IUnitOutputPort, ValueConnection>, IUnitValuePort, IUnitInputPort
    {
        public ValueInput(string key, Type type) : base(key)
        {
            Ensure.That(nameof(type)).IsNotNull(type);

            this.Type = type;
        }

        public Type Type { get; }

        public bool HasDefaultValue => Unit.DefaultValues.ContainsKey(Key);

        public override IEnumerable<ValueConnection> ValidConnections => Unit?.Graph?.ValueConnections.WithDestination(this) ?? Enumerable.Empty<ValueConnection>();

        public override IEnumerable<InvalidConnection> InvalidConnections => Unit?.Graph?.InvalidConnections.WithDestination(this) ?? Enumerable.Empty<InvalidConnection>();

        public override IEnumerable<ValueOutput> ValidConnectedPorts => ValidConnections.Select(c => c.Source);

        public override IEnumerable<IUnitOutputPort> InvalidConnectedPorts => InvalidConnections.Select(c => c.Source);

        // Use for inspector metadata
        //[DoNotSerialize]
        internal object _defaultValue
        {
            get
            {
                return Unit.DefaultValues[Key];
            }
            set
            {
                Unit.DefaultValues[Key] = value;
            }
        }

        public bool nullMeansSelf { get; private set; }

        public bool allowsNull { get; private set; }

        public ValueConnection Connection => Unit.Graph?.ValueConnections.SingleOrDefaultWithDestination(this);

        public override bool HasValidConnection => Connection != null;

        public void SetDefaultValue(object value)
        {
            Ensure.That(nameof(value)).IsOfType(value, Type);

            if (!SupportsDefaultValue(Type))
            {
                return;
            }

            if (Unit.DefaultValues.ContainsKey(Key))
            {
                Unit.DefaultValues[Key] = value;
            }
            else
            {
                Unit.DefaultValues.Add(Key, value);
            }
        }

        public override bool CanConnectToValid(ValueOutput port)
        {
            var source = port;
            var destination = this;

            return source.Type.IsConvertibleTo(destination.Type, false);
        }

        public override void ConnectToValid(ValueOutput port)
        {
            var source = port;
            var destination = this;

            destination.Disconnect();

            Unit.Graph.ValueConnections.Add(new ValueConnection(source, destination));
        }

        public override void ConnectToInvalid(IUnitOutputPort port)
        {
            ConnectInvalid(port, this);
        }

        public override void DisconnectFromValid(ValueOutput port)
        {
            var connection = ValidConnections.SingleOrDefault(c => c.Source == port);

            if (connection != null)
            {
                Unit.Graph.ValueConnections.Remove(connection);
            }
        }

        public override void DisconnectFromInvalid(IUnitOutputPort port)
        {
            DisconnectInvalid(port, this);
        }

        public ValueInput NullMeansSelf()
        {
            nullMeansSelf = true;
            return this;
        }

        public ValueInput AllowsNull()
        {
            if (Type.IsNullable())
            {
                allowsNull = true;
            }
            return this;
        }

        private static readonly HashSet<Type> typesWithDefaultValues = new HashSet<Type>()
        {
            typeof(Color),
            typeof(Type),
        };

        public static bool SupportsDefaultValue(Type type)
        {
            return
                typesWithDefaultValues.Contains(type) ||
                typesWithDefaultValues.Contains(Nullable.GetUnderlyingType(type)) ||
                type.IsBasic();
               
        }

        public override IUnitPort CompatiblePort(IUnit unit)
        {
            if (unit == this.Unit) return null;

            return unit.CompatibleValueOutput(Type);
        }
    }
}
