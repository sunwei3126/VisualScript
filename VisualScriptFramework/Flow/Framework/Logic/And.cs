using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Flow;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework.Logic
{
    public sealed class And: LogicNode
    {
        public ValueInput a { get; private set; }

        public ValueInput b { get; private set; }

        public ValueOutput result { get; private set; }

        protected override void Definition()
        {
            a = ValueInput<bool>(nameof(a));
            b = ValueInput<bool>(nameof(b));
            result = ValueOutput<bool>(nameof(result), Operation).Predictable();
            Requirement(a, result);
            Requirement(b, result);
        }

        private bool Operation(IoTLogic.Flow.Flow flow)
        {
            return flow.GetValue<bool>(a) && flow.GetValue<bool>(b);
        }

    }
}
