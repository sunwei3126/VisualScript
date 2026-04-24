using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Flow;
using VisualScript.Flow.Ports;

namespace VisualScriptFramework.Flow.Framework
{
    public abstract class LoopUnit: Unit
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

        protected abstract ControlOutput Loop(VisualScript.Flow.Flow flow);

        protected abstract IEnumerator LoopCoroutine(VisualScript.Flow.Flow flow);
    }
}
