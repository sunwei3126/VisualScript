using System;
using System.Collections;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Framework
{
    public class ForEach: LoopUnit
    {
        public ValueInput collection { get; private set; }

        public ValueOutput currentIndex { get; private set; }

        public ValueOutput currentKey { get; private set; }

        public ValueOutput currentItem { get; private set; }

        public bool dictionary { get; set; }

        protected override void Definition()
        {
            base.Definition();
            if(dictionary)
            {
                collection = ValueInput<IDictionary>(nameof(collection));
            }
            else
            {
                collection = ValueInput<IEnumerable>(nameof(collection));
            }

            currentIndex = ValueOutput<int>(nameof(currentIndex));

            if(dictionary)
            {
                currentKey = ValueOutput<object>(nameof(currentKey));
            }
          
            currentItem = ValueOutput<object>(nameof(currentItem));

            Requirement(collection, enter);
            Assignment(enter, currentIndex);
            Assignment(enter, currentItem);

            if(dictionary)
            {
                Assignment(enter, currentKey);
            }
        }

        private int Start(IoTLogic.Flow.Flow flow, out IEnumerator enumerator, out IDictionaryEnumerator dictionaryEnumerator, out int currentIndex)
        {
            if(dictionary)
            {
                dictionaryEnumerator = flow.GetValue<IDictionary>(collection).GetEnumerator();
                enumerator = dictionaryEnumerator;
            }
            else
            {
                enumerator = flow.GetValue<IEnumerable>(collection).GetEnumerator();
                dictionaryEnumerator = null;
            }
            currentIndex = -1;
            return flow.EnterLoop();
        }

        private bool MoveNext(IoTLogic.Flow.Flow flow, IEnumerator enumerator, IDictionaryEnumerator dictionaryEnumerator, ref int currentIndex)
        {
            var result = enumerator.MoveNext();
            if(result)
            {
                if(dictionary)
                {
                    flow.SetValue(currentKey, dictionaryEnumerator.Key);
                    flow.SetValue(currentItem, dictionaryEnumerator.Value);
                }
                else
                {
                    flow.SetValue(currentItem, enumerator.Current); 
                }

                currentIndex++;
                flow.SetValue(this.currentIndex, currentIndex);
            }
            return result;
        }
        protected override ControlOutput Loop(IoTLogic.Flow.Flow flow)
        {
            var loop = Start(flow, out var enumerator, out var dictionaryEnumerator, out var currentIndex);

            var stack = flow.PreserveStack();

            try
            {
                while (flow.LoopIsNotBroken(loop) && MoveNext(flow, enumerator, dictionaryEnumerator, ref currentIndex))
                {
                    flow.Invoke(body);
                    flow.RestoreStack(stack);
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }

            flow.DisposePreservedStack(stack);
            flow.ExitLoop(loop);

            return exit;
           
        }

        protected override IEnumerator LoopCoroutine(IoTLogic.Flow.Flow flow)
        {
            var loop = Start(flow, out var enumerator, out var dictionaryEnumerator, out var currentIndex);
            var stack = flow.PreserveStack();
            try
            {
                while(flow.LoopIsNotBroken(loop) && MoveNext(flow, enumerator, dictionaryEnumerator, ref currentIndex))
                {
                    yield return body;
                    flow.RestoreStack(stack);
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
            flow.DisposePreservedStack(stack);
            flow.ExitLoop(loop);

            yield return exit;
        }
    }
}
