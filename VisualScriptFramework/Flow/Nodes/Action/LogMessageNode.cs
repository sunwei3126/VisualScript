using System;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Action
{
    /// <summary>
    /// Writes a formatted message to the console (or any registered log handler).
    /// Useful for debugging logic graphs and auditing IoT events.
    /// </summary>
    public sealed class LogMessageNode : LogicNode
    {
        // ©¤©¤ Control ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public ControlInput  enter { get; private set; }
        public ControlOutput exit  { get; private set; }

        // ©¤©¤ Inputs ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>Message template. Use {0} for the value placeholder.</summary>
        public ValueInput message  { get; private set; }

        /// <summary>Optional value to embed in the message.</summary>
        public ValueInput value    { get; private set; }

        /// <summary>Log level: Info, Warning, Error.</summary>
        public ValueInput level    { get; private set; }

        // ©¤©¤ Outputs ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        /// <summary>The fully formatted log string.</summary>
        public ValueOutput formatted { get; private set; }

        /// <summary>Delegate that receives log output. Defaults to Console.WriteLine.</summary>
        public static Action<LogLevel, string> LogHandler { get; set; } = DefaultLog;

        protected override void Definition()
        {
            enter   = ControlInput(nameof(enter), Execute);
            message = ValueInput<string>(nameof(message), string.Empty);
            value   = ValueInput<object>(nameof(value),   null);
            level   = ValueInput<LogLevel>(nameof(level), LogLevel.Info);

            exit      = ControlOutput(nameof(exit));
            formatted = ValueOutput<string>(nameof(formatted), BuildMessage);

            Requirement(message, enter);
            Requirement(value,   enter);
            Requirement(level,   enter);
            Succession(enter, exit);
        }

        private ControlOutput Execute(Flow flow)
        {
            var msg = BuildMessage(flow);
            var lvl = flow.GetValue<LogLevel>(level);
            LogHandler?.Invoke(lvl, msg);
            return exit;
        }

        private string BuildMessage(Flow flow)
        {
            var template = flow.GetValue<string>(message);
            var val      = flow.GetValue<object>(value);
            try
            {
                return string.IsNullOrEmpty(template)
                    ? (val?.ToString() ?? string.Empty)
                    : string.Format(template, val);
            }
            catch
            {
                return template;
            }
        }

        private static void DefaultLog(LogLevel lvl, string msg)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}][{lvl}] {msg}");
        }
    }

    public enum LogLevel { Info, Warning, Error }
}
