using System;
using IoTLogic.Core.Events;
using IoTLogic.Core.Graph;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Trigger
{
    /// <summary>
    /// Fires on a fixed interval (in seconds). Designed to be driven by an external
    /// scheduler that calls <see cref="Fire"/> at the appropriate time.
    /// </summary>
    public sealed class TimerTriggerNode : LogicNode, IGraphEventListener
    {
        // ©¤©¤ Outputs ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public ControlOutput triggered  { get; private set; }
        public ValueOutput   tickTime   { get; private set; }
        public ValueOutput   tickCount  { get; private set; }

        // ©¤©¤ Configuration ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>Interval between ticks in seconds.</summary>
        public ValueInput intervalSeconds { get; private set; }

        private int _tickCount;
        private DateTime _lastTick = DateTime.MinValue;

        protected override void Definition()
        {
            IsControlRoot = true;

            intervalSeconds = ValueInput<float>(nameof(intervalSeconds), 60f);

            triggered  = ControlOutput(nameof(triggered));
            tickTime   = ValueOutput<DateTime>(nameof(tickTime),  GetTickTime);
            tickCount  = ValueOutput<int>(nameof(tickCount),      GetTickCount);
        }

        // ©¤©¤ IGraphEventListener ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public void StartListening(GraphStack stack)
        {
            stack.GetGraphData<LogicGraphData>().IsListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            stack.GetGraphData<LogicGraphData>().IsListening = false;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetGraphData<LogicGraphData>().IsListening;
        }

        // ©¤©¤ Public API ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>
        /// Returns true if enough time has elapsed since the last tick.
        /// Call this from an external scheduler to decide whether to fire.
        /// </summary>
        public bool ShouldFire(float intervalSec)
        {
            return (DateTime.UtcNow - _lastTick).TotalSeconds >= intervalSec;
        }

        /// <summary>Records a tick and returns the current UTC time.</summary>
        public DateTime Tick()
        {
            _lastTick = DateTime.UtcNow;
            _tickCount++;
            return _lastTick;
        }

        // ©¤©¤ Value resolvers ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        private DateTime GetTickTime(Flow flow)  => _lastTick;
        private int      GetTickCount(Flow flow) => _tickCount;
    }
}
