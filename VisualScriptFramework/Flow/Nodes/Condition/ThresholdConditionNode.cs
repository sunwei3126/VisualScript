using System;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Condition
{
    /// <summary>
    /// Compares a numeric sensor value against a threshold.
    /// Outputs separate control flows for "above", "equal", and "below" branches.
    /// </summary>
    public sealed class ThresholdConditionNode : LogicNode
    {
        // ©¤©¤ Control ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public ControlInput  enter   { get; private set; }
        public ControlOutput above   { get; private set; }
        public ControlOutput equal   { get; private set; }
        public ControlOutput below   { get; private set; }

        // ©¤©¤ Data ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>The sensor / measured value to evaluate.</summary>
        public ValueInput value     { get; private set; }

        /// <summary>Threshold to compare against.</summary>
        public ValueInput threshold { get; private set; }

        /// <summary>Tolerance for equality comparison (default 0).</summary>
        public ValueInput tolerance { get; private set; }

        /// <summary>Boolean output: true when value is above threshold.</summary>
        public ValueOutput isAbove  { get; private set; }

        /// <summary>Boolean output: true when value is below threshold.</summary>
        public ValueOutput isBelow  { get; private set; }

        /// <summary>Difference: value minus threshold.</summary>
        public ValueOutput delta    { get; private set; }

        protected override void Definition()
        {
            enter     = ControlInput(nameof(enter), Execute);
            value     = ValueInput<double>(nameof(value),     0.0);
            threshold = ValueInput<double>(nameof(threshold), 0.0);
            tolerance = ValueInput<double>(nameof(tolerance), 0.0);

            above   = ControlOutput(nameof(above));
            equal   = ControlOutput(nameof(equal));
            below   = ControlOutput(nameof(below));
            isAbove = ValueOutput<bool>(nameof(isAbove), EvalIsAbove);
            isBelow = ValueOutput<bool>(nameof(isBelow), EvalIsBelow);
            delta   = ValueOutput<double>(nameof(delta),  EvalDelta);

            Requirement(value,     enter);
            Requirement(threshold, enter);
            Requirement(tolerance, enter);
            Succession(enter, above);
            Succession(enter, equal);
            Succession(enter, below);
        }

        private ControlOutput Execute(Flow flow)
        {
            var v   = flow.GetValue<double>(value);
            var t   = flow.GetValue<double>(threshold);
            var tol = flow.GetValue<double>(tolerance);
            var diff = v - t;

            if (Math.Abs(diff) <= tol) return equal;
            return diff > 0 ? above : below;
        }

        private bool   EvalIsAbove(Flow flow) => flow.GetValue<double>(value) > flow.GetValue<double>(threshold) + flow.GetValue<double>(tolerance);
        private bool   EvalIsBelow(Flow flow) => flow.GetValue<double>(value) < flow.GetValue<double>(threshold) - flow.GetValue<double>(tolerance);
        private double EvalDelta(Flow flow)   => flow.GetValue<double>(value) - flow.GetValue<double>(threshold);
    }
}
