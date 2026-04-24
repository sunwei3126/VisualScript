using System;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Data
{
    /// <summary>
    /// Provides a constant (compile-time) value into the logic graph.
    /// Supports bool, int, double, and string literals.
    /// </summary>
    public sealed class LiteralNode : LogicNode
    {
        public ValueInput  valueType { get; private set; }
        public ValueInput  rawValue  { get; private set; }
        public ValueOutput output    { get; private set; }

        protected override void Definition()
        {
            valueType = ValueInput<LiteralType>(nameof(valueType), LiteralType.Double);
            rawValue  = ValueInput<string>(nameof(rawValue), "0");

            output = ValueOutput<object>(nameof(output), Resolve).Predictable();

            Requirement(valueType, output);
            Requirement(rawValue,  output);
        }

        private object Resolve(Flow flow)
        {
            var raw  = flow.GetValue<string>(rawValue);
            var type = flow.GetValue<LiteralType>(valueType);

            switch (type)
            {
                case LiteralType.Bool:
                    return bool.TryParse(raw, out var b) ? b : false;
                case LiteralType.Int:
                    return int.TryParse(raw, out var i) ? i : 0;
                case LiteralType.Double:
                    return double.TryParse(raw, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0.0;
                case LiteralType.String:
                    return raw;
                default:
                    return raw;
            }
        }
    }

    public enum LiteralType { Bool, Int, Double, String }
}
