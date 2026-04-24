using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Flow;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework
{
    public abstract class LoopUnit: LogicNode
    {
        public ControlInput enter { get; private set; }

        public ControlOutput exit { get; private set; }

        public ControlOutput body { get; private set; }

        protected override void Definition()
        {
            enter = ControlInputCoroutine(nameof(enter), Loop, LoopCoroutine);
            exit = ControlOutput(nameof(exit));
            body = ControlOutput(nameof(body));

            Succession(enter, body);
            Succession(enter, exit);
        }

        protected abstract ControlOutput Loop(IoTLogic.Flow.Flow flow);

        protected abstract IEnumerator LoopCoroutine(IoTLogic.Flow.Flow flow);
    }
}
