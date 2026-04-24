using System;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Transform
{
    /// <summary>
    /// Linearly maps a numeric value from one range to another
    /// (e.g., ADC 0-4095 ¡ú 0-100% humidity, raw 0-1023 ¡ú 0-5 V).
    /// </summary>
    public sealed class MapValueNode : LogicNode
    {
        public ValueInput  inputValue   { get; private set; }
        public ValueInput  inputMin     { get; private set; }
        public ValueInput  inputMax     { get; private set; }
        public ValueInput  outputMin    { get; private set; }
        public ValueInput  outputMax    { get; private set; }

        /// <summary>Clamp the result to [outputMin, outputMax].</summary>
        public ValueInput  clamp        { get; private set; }

        public ValueOutput outputValue  { get; private set; }
        public ValueOutput normalised   { get; private set; }

        protected override void Definition()
        {
            inputValue = ValueInput<double>(nameof(inputValue), 0.0);
            inputMin   = ValueInput<double>(nameof(inputMin),   0.0);
            inputMax   = ValueInput<double>(nameof(inputMax),   1.0);
            outputMin  = ValueInput<double>(nameof(outputMin),  0.0);
            outputMax  = ValueInput<double>(nameof(outputMax),  100.0);
            clamp      = ValueInput<bool>(nameof(clamp),        true);

            outputValue = ValueOutput<double>(nameof(outputValue), MapOutput);
            normalised  = ValueOutput<double>(nameof(normalised),  Normalise);

            Requirement(inputValue, outputValue);
            Requirement(inputMin,   outputValue);
            Requirement(inputMax,   outputValue);
            Requirement(outputMin,  outputValue);
            Requirement(outputMax,  outputValue);
            Requirement(clamp,      outputValue);
            Requirement(inputValue, normalised);
            Requirement(inputMin,   normalised);
            Requirement(inputMax,   normalised);
        }

        private double MapOutput(Flow flow)
        {
            var t      = Normalise(flow);
            var outMin = flow.GetValue<double>(outputMin);
            var outMax = flow.GetValue<double>(outputMax);
            var result = outMin + t * (outMax - outMin);

            if (flow.GetValue<bool>(clamp))
                result = Math.Max(outMin, Math.Min(outMax, result));

            return result;
        }

        private double Normalise(Flow flow)
        {
            var v   = flow.GetValue<double>(inputValue);
            var min = flow.GetValue<double>(inputMin);
            var max = flow.GetValue<double>(inputMax);
            var range = max - min;
            return range == 0.0 ? 0.0 : (v - min) / range;
        }
    }
}
