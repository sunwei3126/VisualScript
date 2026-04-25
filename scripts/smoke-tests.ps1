Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Step([string]$Name, [scriptblock]$Action)
{
    Write-Output "STEP: $Name"
    & $Action
    Write-Output "OK: $Name"
}

function Assert-True([bool]$Condition, [string]$Message)
{
    if (-not $Condition)
    {
        throw $Message
    }
}

function Assert-Equal($Actual, $Expected, [string]$Message)
{
    if ($Actual -ne $Expected)
    {
        throw "$Message Expected: '$Expected' Actual: '$Actual'."
    }
}

function Assert-Match([string]$Actual, [string]$Pattern, [string]$Message)
{
    if ($Actual -notmatch $Pattern)
    {
        throw "$Message Pattern: '$Pattern' Actual: '$Actual'."
    }
}

function Assert-ThrowsLike([scriptblock]$Action, [string]$Pattern, [string]$Message)
{
    try
    {
        & $Action
    }
    catch
    {
        Assert-Match $_.Exception.Message $Pattern $Message
        return
    }

    throw "$Message Expected an exception matching '$Pattern'."
}

function New-TemperatureGraph
{
    $graph = [IoTLogic.Flow.LogicGraph]::new()
    $graph.Title = "Temperature alarm"
    $graph.Variables.Set("sample", "value")

    $trigger = [IoTLogic.Flow.Nodes.Trigger.DeviceEventTriggerNode]::new()
    $propertyReader = [IoTLogic.Flow.Nodes.Data.DevicePropertyNode]::new()
    $threshold = [IoTLogic.Flow.Nodes.Condition.ThresholdConditionNode]::new()
    $sendCommand = [IoTLogic.Flow.Nodes.Action.SendCommandNode]::new()
    $logAbove = [IoTLogic.Flow.Nodes.Action.LogMessageNode]::new()
    $logNormal = [IoTLogic.Flow.Nodes.Action.LogMessageNode]::new()

    $graph.LogicNodes.Add($trigger)
    $graph.LogicNodes.Add($propertyReader)
    $graph.LogicNodes.Add($threshold)
    $graph.LogicNodes.Add($sendCommand)
    $graph.LogicNodes.Add($logAbove)
    $graph.LogicNodes.Add($logNormal)

    foreach ($node in $graph.LogicNodes)
    {
        $node.EnsureDefined()
    }

    $graph.ControlConnections.Add(
        [IoTLogic.Flow.Connections.ControlConnection]::new($trigger.triggered, $threshold.enter))
    $graph.ValueConnections.Add(
        [IoTLogic.Flow.Connections.ValueConnection]::new($propertyReader.numericValue, $threshold.value))
    $graph.ControlConnections.Add(
        [IoTLogic.Flow.Connections.ControlConnection]::new($threshold.above, $sendCommand.enter))
    $graph.ControlConnections.Add(
        [IoTLogic.Flow.Connections.ControlConnection]::new($sendCommand.exit, $logAbove.enter))
    $graph.ControlConnections.Add(
        [IoTLogic.Flow.Connections.ControlConnection]::new($threshold.below, $logNormal.enter))
    $graph.ValueConnections.Add(
        [IoTLogic.Flow.Connections.ValueConnection]::new($propertyReader.numericValue, $logAbove.value))
    $graph.ValueConnections.Add(
        [IoTLogic.Flow.Connections.ValueConnection]::new($propertyReader.numericValue, $logNormal.value))

    $trigger.DefaultValues["filterEventName"] = "TemperatureReport"
    $propertyReader.DefaultValues["propertyName"] = "temperature"
    $threshold.DefaultValues["threshold"] = 30.0
    $sendCommand.DefaultValues["commandName"] = "TurnOnAC"
    $logAbove.DefaultValues["message"] = "High temperature detected: {0} C. AC command queued."
    $logAbove.DefaultValues["level"] = [IoTLogic.Flow.Nodes.Action.LogLevel]::Warning
    $logNormal.DefaultValues["message"] = "Temperature normal: {0:F1} C"

    return $graph
}

function New-BasicNodeTimerGraph
{
    $graph = [IoTLogic.Flow.LogicGraph]::new()
    $graph.Title = "Timer basic node demo"

    $timer = [IoTLogic.Flow.Nodes.Trigger.TimerTriggerNode]::new()
    $foreach = [IoTLogic.Flow.Framework.ForEach]::new()
    $absolute = [IoTLogic.Flow.Framework.ScalarAbsolute]::new()
    $compare = [IoTLogic.Flow.Nodes.Condition.CompareConditionNode]::new()
    $if = [IoTLogic.Flow.Framework.If]::new()
    $logLarge = [IoTLogic.Flow.Nodes.Action.LogMessageNode]::new()
    $logSmall = [IoTLogic.Flow.Nodes.Action.LogMessageNode]::new()

    $graph.LogicNodes.Add($timer)
    $graph.LogicNodes.Add($foreach)
    $graph.LogicNodes.Add($absolute)
    $graph.LogicNodes.Add($compare)
    $graph.LogicNodes.Add($if)
    $graph.LogicNodes.Add($logLarge)
    $graph.LogicNodes.Add($logSmall)

    foreach ($node in $graph.LogicNodes)
    {
        $node.EnsureDefined()
    }

    $graph.ControlConnections.Add(
        [IoTLogic.Flow.Connections.ControlConnection]::new($timer.triggered, $foreach.enter))
    $graph.ControlConnections.Add(
        [IoTLogic.Flow.Connections.ControlConnection]::new($foreach.body, $if.enter))
    $graph.ControlConnections.Add(
        [IoTLogic.Flow.Connections.ControlConnection]::new($if.ifTrue, $logLarge.enter))
    $graph.ControlConnections.Add(
        [IoTLogic.Flow.Connections.ControlConnection]::new($if.ifFalse, $logSmall.enter))

    $graph.ValueConnections.Add(
        [IoTLogic.Flow.Connections.ValueConnection]::new($foreach.currentItem, $absolute.input))
    $graph.ValueConnections.Add(
        [IoTLogic.Flow.Connections.ValueConnection]::new($absolute.output, $compare.left))
    $graph.ValueConnections.Add(
        [IoTLogic.Flow.Connections.ValueConnection]::new($compare.result, $if.condition))
    $graph.ValueConnections.Add(
        [IoTLogic.Flow.Connections.ValueConnection]::new($absolute.output, $logLarge.value))
    $graph.ValueConnections.Add(
        [IoTLogic.Flow.Connections.ValueConnection]::new($absolute.output, $logSmall.value))

    $foreach.DefaultValues["collection"] = [single[]]@(-3, -1, 4)
    $compare.DefaultValues["right"] = 2.0
    $compare.DefaultValues["operator"] = [IoTLogic.Flow.Nodes.Condition.CompareOperator]::GreaterThan
    $logLarge.DefaultValues["message"] = "abs > 2: {0}"
    $logLarge.DefaultValues["level"] = [IoTLogic.Flow.Nodes.Action.LogLevel]::Warning
    $logSmall.DefaultValues["message"] = "abs <= 2: {0}"

    return $graph
}

$frameworkAssembly = Resolve-Path (Join-Path $PSScriptRoot "..\VisualScriptFramework\bin\Debug\VisualScriptFramework.dll")
Add-Type -Path $frameworkAssembly

Step "graph data creation" {
    $graph = New-TemperatureGraph
    $graphData = $graph.CreateData()

    Assert-Equal $graphData.Variables.Get("sample") "value" "Graph variable clone did not preserve the original value."
    $graphData.Variables.Set("sample", "changed")
    Assert-Equal $graph.Variables.Get("sample") "value" "Graph variable clone must be isolated from the graph definition."
}

Step "graph execution" {
    $graph = New-TemperatureGraph
    $registry = [IoTLogic.Flow.Engine.InMemoryDeviceRegistry]::new()
    $device = $registry.AddDevice("sensor-001", "TemperatureSensor", "temp sensor")
    $device.SetProperty("temperature", 35.5) | Out-Null

    $runner = [IoTLogic.Flow.Engine.LogicGraphRunner]::new($graph, "smoke-test")

    try
    {
        $runner.Start()

        $highEvent = [IoTLogic.Domain.DeviceEvent]::new(
            "sensor-001",
            "TemperatureSensor",
            "TemperatureReport",
            35.5,
            [DateTime]::UtcNow)
        $highContext = [IoTLogic.Domain.TriggerContext]::new($highEvent, $device)
        $highResult = $runner.Execute($highContext)

        Assert-True $highResult.Succeeded "High-temperature execution should succeed."
        Assert-Equal $highResult.Commands.Count 1 "High-temperature execution should enqueue exactly one command."
        Assert-Equal $highResult.Commands[0].CommandName "TurnOnAC" "Unexpected command produced by the graph."

        $device.SetProperty("temperature", 22.0) | Out-Null
        $lowEvent = [IoTLogic.Domain.DeviceEvent]::new(
            "sensor-001",
            "TemperatureSensor",
            "TemperatureReport",
            22.0,
            [DateTime]::UtcNow)
        $lowContext = [IoTLogic.Domain.TriggerContext]::new($lowEvent, $device)
        $lowResult = $runner.Execute($lowContext)

        Assert-True $lowResult.Succeeded "Low-temperature execution should succeed."
        Assert-Equal $lowResult.Commands.Count 0 "Low-temperature execution should not enqueue commands."
    }
    finally
    {
        $runner.Dispose()
    }
}

Step "timer basic nodes execution" {
    $graph = New-BasicNodeTimerGraph
    $runner = [IoTLogic.Flow.Engine.LogicGraphRunner]::new($graph, "timer-basic-nodes")
    $messages = [System.Collections.Generic.List[string]]::new()
    $previousLogHandler = [IoTLogic.Flow.Nodes.Action.LogMessageNode]::LogHandler

    [IoTLogic.Flow.Nodes.Action.LogMessageNode]::LogHandler =
        [System.Action[IoTLogic.Flow.Nodes.Action.LogLevel,string]] {
            param($level, $message)
            $messages.Add("$level|$message")
        }

    try
    {
        $runner.Start()
        $result = $runner.ExecuteTimerTicks()

        Assert-True $result.Succeeded "Timer graph execution should succeed."
        Assert-Equal $messages.Count 3 "ForEach should process all three demo values."
        Assert-Match ($messages -join "`n") "Warning\|abs > 2: 3" "The true branch should log the absolute value of -3."
        Assert-Match ($messages -join "`n") "Info\|abs <= 2: 1" "The false branch should log the absolute value of -1."
        Assert-Match ($messages -join "`n") "Warning\|abs > 2: 4" "The true branch should log the absolute value of 4."
    }
    finally
    {
        [IoTLogic.Flow.Nodes.Action.LogMessageNode]::LogHandler = $previousLogHandler
        $runner.Dispose()
    }
}

Step "json export and round-trip" {
    $graph = New-TemperatureGraph
    $document = [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Export($graph)

    Assert-Equal $document.SchemaVersion 1 "Unexpected schema version."
    Assert-Equal $document.Nodes.Count 6 "Unexpected node count in exported document."
    Assert-Equal $document.Edges.Count 7 "Unexpected edge count in exported document."
    Assert-Equal $document.Nodes[0].Type "nodes/trigger/device-event-trigger" "Unexpected exported node type key."

    $logAboveDocument = $document.Nodes | Where-Object { $_.Defaults["message"] -eq "High temperature detected: {0} C. AC command queued." } | Select-Object -First 1
    Assert-True ($null -ne $logAboveDocument) "Expected high-temperature log node in exported document."
    Assert-Equal $logAboveDocument.Defaults["level"]['$kind'] "enum" "Enum defaults must be tagged."
    Assert-Equal $logAboveDocument.Defaults["level"]["type"] "IoTLogic.Flow.Nodes.Action.LogLevel" "Enum default type should be serialized."
    Assert-Equal $logAboveDocument.Defaults["level"]["value"] "Warning" "Enum default value should be serialized."

    $jsonPath = Join-Path $env:TEMP "iot-logic-graph.json"
    [IoTLogic.Flow.Serialization.LogicGraphDocumentSerializer]::Save($jsonPath, $document)
    $json = Get-Content -Raw $jsonPath

    Assert-Match $json '"schemaVersion"\s*:\s*1' "Saved JSON must contain schemaVersion."
    Assert-Match $json '"type"\s*:\s*"nodes/trigger/device-event-trigger"' "Saved JSON must contain stable node type keys."

    $reloadedDocument = [IoTLogic.Flow.Serialization.LogicGraphDocumentSerializer]::Load($jsonPath)
    $reloadedGraph = [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Compile($reloadedDocument)

    Assert-Equal $reloadedGraph.Title "Temperature alarm" "Graph title should survive JSON round-trip."
    Assert-Equal $reloadedGraph.ControlConnections.Count 4 "Control connection count should survive JSON round-trip."
    Assert-Equal $reloadedGraph.ValueConnections.Count 3 "Value connection count should survive JSON round-trip."

    $registry = [IoTLogic.Flow.Engine.InMemoryDeviceRegistry]::new()
    $device = $registry.AddDevice("sensor-001", "TemperatureSensor", "temp sensor")
    $device.SetProperty("temperature", 35.5) | Out-Null
    $runner = [IoTLogic.Flow.Engine.LogicGraphRunner]::new($reloadedGraph, "json-roundtrip")

    try
    {
        $runner.Start()
        $highEvent = [IoTLogic.Domain.DeviceEvent]::new(
            "sensor-001",
            "TemperatureSensor",
            "TemperatureReport",
            35.5,
            [DateTime]::UtcNow)
        $highContext = [IoTLogic.Domain.TriggerContext]::new($highEvent, $device)
        $highResult = $runner.Execute($highContext)
        Assert-True $highResult.Succeeded "Reloaded graph should execute successfully."
        Assert-Equal $highResult.Commands.Count 1 "Reloaded graph should still enqueue one command for a high temperature."

        $device.SetProperty("temperature", 22.0) | Out-Null
        $lowEvent = [IoTLogic.Domain.DeviceEvent]::new(
            "sensor-001",
            "TemperatureSensor",
            "TemperatureReport",
            22.0,
            [DateTime]::UtcNow)
        $lowContext = [IoTLogic.Domain.TriggerContext]::new($lowEvent, $device)
        $lowResult = $runner.Execute($lowContext)
        Assert-True $lowResult.Succeeded "Reloaded graph should execute successfully for low temperature."
        Assert-Equal $lowResult.Commands.Count 0 "Reloaded graph should not enqueue commands for a low temperature."
    }
    finally
    {
        $runner.Dispose()
    }
}

Step "json validation failures" {
    $unknownType = [IoTLogic.Flow.Serialization.LogicGraphDocument]::new()
    $unknownType.Nodes.Add([IoTLogic.Flow.Serialization.LogicGraphNodeDocument]::new())
    $unknownType.Nodes[0].Id = [Guid]::NewGuid().ToString("D")
    $unknownType.Nodes[0].Type = "nodes/unknown/missing"
    Assert-ThrowsLike { [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Compile($unknownType) } "unknown node type" "Unknown node types must fail."

    $duplicateId = [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Export((New-TemperatureGraph))
    $duplicateId.Nodes[1].Id = $duplicateId.Nodes[0].Id
    Assert-ThrowsLike { [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Compile($duplicateId) } "duplicate node id" "Duplicate node ids must fail."

    $missingPort = [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Export((New-TemperatureGraph))
    $missingPort.Edges[0].ToPort = "missingPort"
    Assert-ThrowsLike { [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Compile($missingPort) } "unknown port" "Missing ports must fail."

    $wrongVersion = [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Export((New-TemperatureGraph))
    $wrongVersion.SchemaVersion = 99
    Assert-ThrowsLike { [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Compile($wrongVersion) } "schema version" "Mismatched schema versions must fail."

    $unsupportedValue = [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Export((New-TemperatureGraph))
    $unsupportedValue.Variables.Add([IoTLogic.Flow.Serialization.LogicGraphVariableDocument]::new())
    $unsupportedValue.Variables[-1].Name = "bad"
    $unsupportedValue.Variables[-1].Value = [Newtonsoft.Json.Linq.JArray]::new()
    Assert-ThrowsLike { [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Compile($unsupportedValue) } "unsupported" "Unsupported JSON value shapes must fail."

    $subgraphGraph = [IoTLogic.Flow.LogicGraph]::new()
    $subgraphGraph.LogicNodes.Add([IoTLogic.Flow.SubgraphLogicNode]::WithInputOutput())
    Assert-ThrowsLike { [IoTLogic.Flow.Serialization.LogicGraphDocumentCompiler]::Export($subgraphGraph) } "subgraph" "Subgraphs must be rejected by JSON export."
}

Write-Output "Smoke tests passed."
