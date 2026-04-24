using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Core.Ensure;
using IoTLogic.Core.Reflection;
using IoTLogic.Core.Utities;
using IoTLogic.Flow;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework
{
    public sealed class Literal: LogicNode
    {
        public Literal() : base() { }
        public Literal(Type type) : this(type, type.PseudoDefault()) { }

        public Literal(Type type, object value): base()
        {
            Ensure.That(nameof(type)).IsNotNull(type);
            Ensure.That(nameof(value)).IsOfType(value, type);
        }

        public override bool CanDefine => type != null;

        private object _value;

        public Type type { get; internal set; }

        public object value
        {
            get => _value;
            set
            {
                Ensure.That(nameof(value)).IsOfType(value, type);
                _value = value;
            }
        }

        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            output = ValueOutput(type, nameof(output), (flow) => value).Predictable();
        }

        #region Analytics

        public override AnalyticsIdentifier GetAnalyticsIdentifier()
        {
            var aid = new AnalyticsIdentifier
            {
                Identifier = $"{GetType().FullName}({type.Name})",
                Namespace = type.Namespace,
            };
            aid.Hashcode = aid.Identifier.GetHashCode();
            return aid;
        }

        #endregion

    }
}
