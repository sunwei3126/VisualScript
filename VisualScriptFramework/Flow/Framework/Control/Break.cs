using IoTLogic.Flow;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework
{
    public class Break: LogicNode
    {
        public ControlInput enter { get; set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Operation);
        }

        public ControlOutput Operation(IoTLogic.Flow.Flow flow)
        {
            flow.BreakLoop();
            return null;
        }
    }
}
