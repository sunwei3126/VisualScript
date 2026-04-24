using VisualScript.Flow;
using VisualScript.Flow.Ports;

namespace VisualScriptFramework.Flow.Framework
{
    public class Break: Unit
    {
        public ControlInput enter { get; set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Operation);
        }

        public ControlOutput Operation(VisualScript.Flow.Flow flow)
        {
            flow.BreakLoop();
            return null;
        }
    }
}
