using System;
using IoTLogic.Domain;
using IoTLogic.Flow;
using IoTLogic.Flow.Connections;
using IoTLogic.Flow.Engine;
using IoTLogic.Flow.Nodes.Action;
using IoTLogic.Flow.Nodes.Condition;
using IoTLogic.Flow.Nodes.Data;
using IoTLogic.Flow.Nodes.Trigger;
using IoTLogic.Flow.Serialization;

namespace IoTLogic
{
    internal static class Program
    {
        private const string DeviceId = "sensor-001";
        private const string ProductKey = "TemperatureSensor";
        private const string EventName = "TemperatureReport";

        private static void Main(string[] args)
        {
            Console.WriteLine("=== IoT Visual Logic Engine ===");

            var registry = new InMemoryDeviceRegistry();
            var sensor = registry.AddDevice(DeviceId, ProductKey, "Temperature sensor");
            sensor.SetProperty("temperature", 35.5);
            sensor.SetProperty("humidity", 60.0);

            var options = ParseOptions(args);
            var graph = BuildTemperatureAlarmGraph();

            if (!string.IsNullOrWhiteSpace(options.ExportSamplePath))
            {
                LogicGraphDocumentSerializer.Save(
                    options.ExportSamplePath,
                    LogicGraphDocumentCompiler.Export(graph));
                Console.WriteLine($"Exported sample graph to: {options.ExportSamplePath}");
            }

            if (!string.IsNullOrWhiteSpace(options.GraphPath))
            {
                graph = LogicGraphDocumentCompiler.Compile(
                    LogicGraphDocumentSerializer.Load(options.GraphPath));
                Console.WriteLine($"Loaded graph from JSON: {options.GraphPath}");
            }

            using (var dispatcher = new TriggerDispatcher(registry))
            {
                dispatcher.Register(graph, "Temperature alarm");
                dispatcher.OnExecuted += result =>
                    Console.WriteLine($"\n[Result] {result}");
                dispatcher.CommandHandler = (commands, _) =>
                {
                    foreach (var command in commands)
                    {
                        Console.WriteLine($"  -> Command: {command}");
                    }
                };

                dispatcher.Start();

                RunScenario(dispatcher, sensor, 35.5, "Above threshold");
                RunScenario(dispatcher, sensor, 22.0, "Normal range");
            }

            Console.WriteLine("\n=== Complete ===");

            if (options.Pause)
            {
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
        }

        private static LogicGraph BuildTemperatureAlarmGraph()
        {
            var graph = new LogicGraph { Title = "Temperature alarm" };

            var trigger = new DeviceEventTriggerNode();
            var propertyReader = new DevicePropertyNode();
            var threshold = new ThresholdConditionNode();
            var sendCommand = new SendCommandNode();
            var logAbove = new LogMessageNode();
            var logNormal = new LogMessageNode();

            graph.LogicNodes.Add(trigger);
            graph.LogicNodes.Add(propertyReader);
            graph.LogicNodes.Add(threshold);
            graph.LogicNodes.Add(sendCommand);
            graph.LogicNodes.Add(logAbove);
            graph.LogicNodes.Add(logNormal);

            foreach (var node in graph.LogicNodes)
            {
                node.EnsureDefined();
            }

            graph.ControlConnections.Add(new ControlConnection(trigger.triggered, threshold.enter));
            graph.ValueConnections.Add(new ValueConnection(propertyReader.numericValue, threshold.value));
            graph.ControlConnections.Add(new ControlConnection(threshold.above, sendCommand.enter));
            graph.ControlConnections.Add(new ControlConnection(sendCommand.exit, logAbove.enter));
            graph.ControlConnections.Add(new ControlConnection(threshold.below, logNormal.enter));
            graph.ValueConnections.Add(new ValueConnection(propertyReader.numericValue, logAbove.value));
            graph.ValueConnections.Add(new ValueConnection(propertyReader.numericValue, logNormal.value));

            trigger.DefaultValues["filterEventName"] = EventName;
            propertyReader.DefaultValues["propertyName"] = "temperature";
            threshold.DefaultValues["threshold"] = 30.0;
            sendCommand.DefaultValues["commandName"] = "TurnOnAC";
            logAbove.DefaultValues["message"] = "High temperature detected: {0} C. AC command queued.";
            logAbove.DefaultValues["level"] = LogLevel.Warning;
            logNormal.DefaultValues["message"] = "Temperature normal: {0:F1} C";

            return graph;
        }

        private static void RunScenario(
            TriggerDispatcher dispatcher,
            SimpleDevice sensor,
            double temperature,
            string label)
        {
            sensor.SetProperty("temperature", temperature);

            Console.WriteLine($"\n--- {label}: {temperature:F1} C ---");
            dispatcher.Dispatch(new DeviceEvent(
                DeviceId,
                ProductKey,
                EventName,
                temperature,
                DateTime.UtcNow));
        }

        private static Options ParseOptions(string[] args)
        {
            var options = new Options();

            if (args == null)
            {
                return options;
            }

            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];

                if (string.Equals(arg, "--pause", StringComparison.OrdinalIgnoreCase))
                {
                    options.Pause = true;
                    continue;
                }

                if (string.Equals(arg, "--graph", StringComparison.OrdinalIgnoreCase))
                {
                    options.GraphPath = ReadOptionValue(args, ref index, arg);
                    continue;
                }

                if (string.Equals(arg, "--export-sample", StringComparison.OrdinalIgnoreCase))
                {
                    options.ExportSamplePath = ReadOptionValue(args, ref index, arg);
                    continue;
                }
            }

            return options;
        }

        private static string ReadOptionValue(string[] args, ref int index, string optionName)
        {
            var valueIndex = index + 1;

            if (valueIndex >= args.Length || string.IsNullOrWhiteSpace(args[valueIndex]))
            {
                throw new ArgumentException($"Option '{optionName}' requires a file path value.");
            }

            index = valueIndex;
            return args[valueIndex];
        }

        private sealed class Options
        {
            public string ExportSamplePath { get; set; }

            public string GraphPath { get; set; }

            public bool Pause { get; set; }
        }
    }
}
