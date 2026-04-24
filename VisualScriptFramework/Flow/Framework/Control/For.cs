using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Flow.Ports;

namespace VisualScriptFramework.Flow.Framework
{
    public sealed class For: LoopUnit
    {
        public ValueInput firstIndex { get; private set; }

        public ValueInput lastIndex { get;private set; }

        public ValueInput step { get; private set; }    

        public ValueOutput currentIndex { get; private set; }

        protected override void Definition()
        {
            firstIndex = ValueInput(nameof(firstIndex), 0);
            lastIndex = ValueInput(nameof(lastIndex), 10);
            step = ValueInput(nameof(step), 1);
            currentIndex = ValueOutput<int>(nameof(currentIndex));

            base.Definition();

            Requirement(firstIndex, enter);
            Requirement(lastIndex, enter);
            Requirement(step, enter);
            Assignment(enter, currentIndex);
        }
        private int Start(VisualScript.Flow.Flow flow, out int currentIndex, out int lastIndex, out bool ascending)
        {
            var firstIndex = flow.GetValue<int>(this.firstIndex);
            lastIndex = flow.GetValue<int>(this.lastIndex);

            ascending = firstIndex <= lastIndex;
            currentIndex = firstIndex;
            flow.SetValue(this.currentIndex, currentIndex);
            return flow.EnterLoop();
        }
        protected override ControlOutput Loop(VisualScript.Flow.Flow flow)
        {
            var loop = Start(flow, out int currentIndex, out int lastIndex, out bool ascending);
            if(!IsStepValueZero())
            {
                var stack = flow.PreserveStack();

                while(flow.LoopIsNotBroken(loop) && CanMoveNext(currentIndex,lastIndex, ascending))
                {
                    flow.Invoke(body);

                    flow.RestoreStack(stack);

                    MoveNext(flow, ref currentIndex);
                }

                flow.DisposePreservedStack(stack);
            }

            flow.ExitLoop(loop);
            return exit;
        }

        protected override IEnumerator LoopCoroutine(VisualScript.Flow.Flow flow)
        {
            var loop = Start(flow, out int currentIndex, out int lastIndex, out bool ascending);
            var stack = flow.PreserveStack();
            while(flow.LoopIsNotBroken(loop) && CanMoveNext(currentIndex,lastIndex,ascending))
            {
                yield return body;
                flow.RestoreStack(stack);
                MoveNext(flow, ref currentIndex);
            }

            flow.DisposePreservedStack(stack);
            flow.ExitLoop(loop);

            yield return exit;
        }

        private bool CanMoveNext(int currentIndex, int lastIndex, bool ascending)
        {
            if(ascending)
            {
                return currentIndex < lastIndex;
            }
            else
            {
                return currentIndex > lastIndex;
            }
        }

        private void MoveNext(VisualScript.Flow.Flow flow, ref int currentIndex)
        {
            currentIndex += flow.GetValue<int>(step);
            flow.SetValue(this.currentIndex, currentIndex);
        }

        public bool IsStepValueZero()
        {
            var isDefaultZero = !step.HasValidConnection && (int)DefaultValues[step.Key] == 0;
            var isConnectedToLiteralZero = false;

            if(step.HasValidConnection && step.Connection.Source.Unit is Literal literal)
            {
                if(Convert.ToInt32(literal.value) == 0)
                {
                    isConnectedToLiteralZero = true;
                }
            }
            return isDefaultZero || isConnectedToLiteralZero;
        }
    }
}
