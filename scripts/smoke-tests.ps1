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
    $logNormal.DefaultValues["message"] = "Temperature normal: {0:F1} C"

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

Write-Output "Smoke tests passed."
