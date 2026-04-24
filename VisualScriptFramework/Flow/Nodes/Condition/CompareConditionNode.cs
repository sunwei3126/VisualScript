using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Condition
{
    /// <summary>
    /// Generic value comparison node. Supports ==, !=, &gt;, &gt;=, &lt;, &lt;=.
    /// Routes control flow based on result, and exposes a boolean output.
    /// </summary>
    public sealed class CompareConditionNode : LogicNode
    {
        // ©¤©¤ Control ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public ControlInput  enter    { get; private set; }
        public ControlOutput isTrue   { get; private set; }
        public ControlOutput isFalse  { get; private set; }

        // ©¤©¤ Data ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public ValueInput  left      { get; private set; }
        public ValueInput  right     { get; private set; }
        public ValueInput  @operator { get; private set; }
        public ValueOutput result    { get; private set; }

        protected override void Definition()
        {
            enter    = ControlInput(nameof(enter), Execute);
            left     = ValueInput<double>(nameof(left),     0.0);
            right    = ValueInput<double>(nameof(right),    0.0);
            @operator = ValueInput<CompareOperator>(nameof(@operator), CompareOperator.Equal);

            isTrue  = ControlOutput(nameof(isTrue));
            isFalse = ControlOutput(nameof(isFalse));
            result  = ValueOutput<bool>(nameof(result), Evaluate);

            Requirement(left,      enter);
            Requirement(right,     enter);
            Requirement(@operator, enter);
            Succession(enter, isTrue);
            Succession(enter, isFalse);
        }

        private ControlOutput Execute(Flow flow) =>
            Evaluate(flow) ? isTrue : isFalse;

        private bool Evaluate(Flow flow)
        {
            var l = flow.GetValue<double>(left);
            var r = flow.GetValue<double>(right);
            var op = flow.GetValue<CompareOperator>(@operator);
            return Compare(l, r, op);
        }

        private static bool Compare(double l, double r, CompareOperator op)
        {
            switch (op)
            {
                case CompareOperator.Equal:              return l == r;
                case CompareOperator.NotEqual:           return l != r;
                case CompareOperator.GreaterThan:        return l > r;
                case CompareOperator.GreaterThanOrEqual: return l >= r;
                case CompareOperator.LessThan:           return l < r;
                case CompareOperator.LessThanOrEqual:    return l <= r;
                default:                                 return false;
            }
        }
    }

    public enum CompareOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }
}
