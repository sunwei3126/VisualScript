using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Flow;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework
{
    public abstract class Absolute<TInput>: LogicNode
    {
        public ValueInput input { get; private set; }

        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            input = ValueInput<TInput>(nameof(input));
            output = ValueOutput(nameof(output), Operation).Predictable();

            Requirement(input, output);
        }

        protected abstract TInput Operation(TInput input);

        public TInput Operation(IoTLogic.Flow.Flow flow)
        {
            return Operation(flow.GetValue<TInput>(input));
        }
    }
}
