using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorLogic.Models;
using NodeEditorLogic.ViewModels;
using NodeEditorLogic.ViewModels.Nodes;

namespace NodeEditorLogic.Services;

public sealed class LogicSimulationResult
{
    public int Iterations { get; init; }
    public bool IsStable { get; init; }
    public IReadOnlyList<string> Messages { get; init; } = Array.Empty<string>();
}

public sealed class LogicSimulationService
{
    private sealed class NodeTimingState
    {
        public LogicValue[][]? PendingOutputs;
        public int RemainingDelay;
    }

    private readonly LogicComponentLibrary _library;
    private readonly Dictionary<LogicNodeContentViewModel, NodeTimingState> _timingStates = new();

    public LogicSimulationService(LogicComponentLibrary library)
    {
        _library = library;
    }

    public int TickCount { get; private set; }

    public LogicSimulationResult Evaluate(IDrawingNode drawing)
    {
        return EvaluateInternal(drawing, applyDelay: false, advanceSequential: false);
    }

    public LogicSimulationResult Step(IDrawingNode drawing)
    {
        TickCount++;
        AdvanceClocks(drawing);
        return EvaluateInternal(drawing, applyDelay: true, advanceSequential: true);
    }

    public void Reset(IDrawingNode drawing)
    {
        TickCount = 0;
        _timingStates.Clear();

        foreach (var (_, content) in GetLogicNodes(drawing))
        {
            if (content is LogicClockNodeViewModel clock)
            {
                clock.Counter = 0;
                clock.State = LogicValue.Low;
            }
            else if (content is LogicFlipFlopNodeViewModel flipFlop)
            {
                flipFlop.StoredValue = LogicValue.Low;
                flipFlop.LastClockValue = LogicValue.Low;
            }
        }

        Evaluate(drawing);
    }

    private LogicSimulationResult EvaluateInternal(IDrawingNode drawing, bool applyDelay, bool advanceSequential)
    {
        var messages = new HashSet<string>();
        var iterations = 0;
        var maxIterations = applyDelay ? 4 : 12;

        PruneStates(drawing);
        ResetConnectorStatus(drawing);

        if (applyDelay)
        {
            var pendingChanged = false;
            ApplyPendingOutputs(drawing, ref pendingChanged);
        }

        var keepGoing = true;
        while (keepGoing && iterations < maxIterations)
        {
            keepGoing = false;
            var changed = false;
            iterations++;

            foreach (var (node, content) in GetLogicNodes(drawing))
            {
                EnsurePinLists(node, content);
                UpdateInputPins(drawing, content, messages, ref changed);
            }

            if (advanceSequential && iterations == 1)
            {
                UpdateFlipFlops(drawing);
            }

            foreach (var (_, content) in GetLogicNodes(drawing))
            {
                UpdateOutputs(content, messages, applyDelay, ref changed);
            }

            foreach (var (_, content) in GetLogicNodes(drawing))
            {
                if (content is LogicOutputNodeViewModel output)
                {
                    output.ObservedValue = output.InputPins.Count > 0 ? output.InputPins[0].Value : LogicValue.Unknown;
                }
                else if (content is LogicBusOutputNodeViewModel busOutput)
                {
                    if (busOutput.InputPins.Count > 0)
                    {
                        var busValue = busOutput.InputPins[0].BusValue;
                        busOutput.ObservedText = LogicSignalHelper.ToHexString(busValue);
                    }
                    else
                    {
                        busOutput.ObservedText = "X";
                    }
                }
            }

            if (changed)
            {
                keepGoing = true;
            }
        }

        var isStable = iterations < maxIterations;

        if (!applyDelay && iterations >= maxIterations)
        {
            messages.Add("Simulation did not settle after 12 iterations.");
        }
        else if (applyDelay && iterations >= maxIterations)
        {
            messages.Add("Zero-delay loop did not settle in this tick.");
        }

        return new LogicSimulationResult
        {
            Iterations = iterations,
            IsStable = isStable,
            Messages = messages.Count == 0 ? Array.Empty<string>() : new List<string>(messages)
        };
    }

    private void ApplyPendingOutputs(IDrawingNode drawing, ref bool changed)
    {
        foreach (var (_, content) in GetLogicNodes(drawing))
        {
            if (!_timingStates.TryGetValue(content, out var state) || state.PendingOutputs is null)
            {
                continue;
            }

            state.RemainingDelay--;

            if (state.RemainingDelay <= 0)
            {
                ApplyOutputs(content, state.PendingOutputs, ref changed);
                state.PendingOutputs = null;
                state.RemainingDelay = 0;
            }
        }
    }

    private void AdvanceClocks(IDrawingNode drawing)
    {
        foreach (var (_, content) in GetLogicNodes(drawing))
        {
            if (content is not LogicClockNodeViewModel clock)
            {
                continue;
            }

            if (!clock.IsRunning)
            {
                continue;
            }

            var period = Math.Max(1, clock.Period);
            var highTicks = Math.Clamp(clock.HighTicks, 1, period);

            clock.State = clock.Counter < highTicks ? LogicValue.High : LogicValue.Low;
            clock.Counter++;

            if (clock.Counter >= period)
            {
                clock.Counter = 0;
            }
        }
    }

    private void UpdateFlipFlops(IDrawingNode drawing)
    {
        foreach (var (_, content) in GetLogicNodes(drawing))
        {
            if (content is not LogicFlipFlopNodeViewModel flipFlop)
            {
                continue;
            }

            var dataPin = flipFlop.InputPins.Count > 0 ? flipFlop.InputPins[0] : null;
            var clockPin = flipFlop.InputPins.Count > 1 ? flipFlop.InputPins[1] : null;

            var clockValue = clockPin?.Value ?? LogicValue.Unknown;
            if (flipFlop.LastClockValue == LogicValue.Low && clockValue == LogicValue.High)
            {
                flipFlop.StoredValue = dataPin?.Value ?? LogicValue.Unknown;
            }

            flipFlop.LastClockValue = clockValue;
        }
    }

    private void PruneStates(IDrawingNode drawing)
    {
        if (drawing.Nodes is null)
        {
            _timingStates.Clear();
            return;
        }

        var alive = new HashSet<LogicNodeContentViewModel>();
        foreach (var node in drawing.Nodes)
        {
            if (node is NodeViewModel viewModel && viewModel.Content is LogicNodeContentViewModel content)
            {
                alive.Add(content);
            }
        }

        var toRemove = new List<LogicNodeContentViewModel>();
        foreach (var state in _timingStates.Keys)
        {
            if (!alive.Contains(state))
            {
                toRemove.Add(state);
            }
        }

        foreach (var key in toRemove)
        {
            _timingStates.Remove(key);
        }
    }

    private static void ResetConnectorStatus(IDrawingNode drawing)
    {
        if (drawing.Connectors is null)
        {
            return;
        }

        foreach (var connector in drawing.Connectors)
        {
            if (connector is not LogicConnectorViewModel logicConnector)
            {
                continue;
            }

            logicConnector.IsInvalid = false;
            logicConnector.IsContention = false;
            logicConnector.StatusMessage = null;

            if (connector.Start is LogicPinViewModel start && connector.End is LogicPinViewModel end)
            {
                logicConnector.BusWidth = Math.Max(start.BusWidth, end.BusWidth);
                logicConnector.IsBus = logicConnector.BusWidth > 1;

                if (!TryResolveConnection(connector, out _, out _, out var reason))
                {
                    logicConnector.IsInvalid = true;
                    logicConnector.StatusMessage = reason;
                }
            }
            else
            {
                logicConnector.BusWidth = 1;
                logicConnector.IsBus = false;
            }
        }
    }

    private static IEnumerable<(NodeViewModel Node, LogicNodeContentViewModel Content)> GetLogicNodes(IDrawingNode drawing)
    {
        if (drawing.Nodes is null)
        {
            yield break;
        }

        foreach (var node in drawing.Nodes)
        {
            if (node is NodeViewModel viewModel && viewModel.Content is LogicNodeContentViewModel content)
            {
                yield return (viewModel, content);
            }
        }
    }

    private static void EnsurePinLists(NodeViewModel node, LogicNodeContentViewModel content)
    {
        content.HostNode = node;

        if (content.InputPins.Count > 0 || content.OutputPins.Count > 0)
        {
            return;
        }

        if (node.Pins is null)
        {
            return;
        }

        foreach (var pin in node.Pins)
        {
            if (pin is not LogicPinViewModel logicPin)
            {
                continue;
            }

            if (logicPin.Kind == LogicPinKind.Input)
            {
                content.InputPins.Add(logicPin);
            }
            else
            {
                content.OutputPins.Add(logicPin);
            }
        }
    }

    private void UpdateOutputs(LogicNodeContentViewModel content, ISet<string> messages, bool applyDelay, ref bool changed)
    {
        if (content is LogicOutputNodeViewModel || content is LogicBusOutputNodeViewModel)
        {
            return;
        }

        var desiredOutputs = ComputeOutputs(content, messages);
        if (desiredOutputs is null)
        {
            return;
        }

        if (!applyDelay)
        {
            ApplyOutputs(content, desiredOutputs, ref changed);
            return;
        }

        var state = GetTimingState(content);
        if (state.PendingOutputs is not null && OutputsEqual(state.PendingOutputs, desiredOutputs))
        {
            return;
        }

        if (OutputsEqual(content.OutputPins, desiredOutputs) && state.PendingOutputs is null)
        {
            return;
        }

        state.PendingOutputs = desiredOutputs;
        state.RemainingDelay = Math.Max(0, content.PropagationDelay);

        if (state.RemainingDelay == 0)
        {
            ApplyOutputs(content, desiredOutputs, ref changed);
            state.PendingOutputs = null;
        }
    }

    private LogicValue[][]? ComputeOutputs(LogicNodeContentViewModel content, ISet<string> messages)
    {
        switch (content)
        {
            case LogicInputNodeViewModel input:
                return WrapOutputs(CreateUniformOutputs(input.OutputPins.Count, input.IsOn ? LogicValue.High : LogicValue.Low));
            case LogicClockNodeViewModel clock:
                return WrapOutputs(CreateUniformOutputs(clock.OutputPins.Count, clock.State));
            case LogicFlipFlopNodeViewModel flipFlop:
                if (flipFlop.InputPins.Any(pin => pin.BusWidth > 1) || flipFlop.OutputPins.Any(pin => pin.BusWidth > 1))
                {
                    messages.Add($"Bus pins are not supported on {flipFlop.Title}.");
                    return WrapOutputs(CreateUnknownOutputs(flipFlop.OutputPins.Count));
                }

                return WrapOutputs(GetFlipFlopOutputs(flipFlop));
            case LogicGateNodeViewModel gate:
                return WrapOutputs(ComputeGateOutputs(gate, messages));
            case LogicBusInputNodeViewModel busInput:
                return new[] { LogicSignalHelper.CreateBusFromInt(busInput.BusValue, busInput.BusWidth) };
            case LogicBusSplitNodeViewModel busSplit:
                return ComputeBusSplitOutputs(busSplit);
            case LogicBusMergeNodeViewModel busMerge:
                return ComputeBusMergeOutputs(busMerge);
            default:
                return null;
        }
    }

    private static LogicValue[][] WrapOutputs(LogicValue[] outputs)
    {
        if (outputs.Length == 0)
        {
            return Array.Empty<LogicValue[]>();
        }

        var wrapped = new LogicValue[outputs.Length][];
        for (var i = 0; i < outputs.Length; i++)
        {
            wrapped[i] = new[] { outputs[i] };
        }

        return wrapped;
    }

    private static LogicValue[][] ComputeBusSplitOutputs(LogicBusSplitNodeViewModel split)
    {
        if (split.InputPins.Count == 0 || split.OutputPins.Count == 0)
        {
            return Array.Empty<LogicValue[]>();
        }

        var input = split.InputPins[0];
        var width = Math.Max(1, split.BitCount);
        var inputSignal = input.BusWidth > 1 ? input.BusValue : new[] { input.Value };
        if (inputSignal.Length != width)
        {
            inputSignal = LogicSignalHelper.CreateUnknownBus(width);
        }

        var outputs = new LogicValue[split.OutputPins.Count][];
        for (var i = 0; i < outputs.Length; i++)
        {
            var bit = i < inputSignal.Length ? inputSignal[i] : LogicValue.Unknown;
            outputs[i] = new[] { bit };
        }

        return outputs;
    }

    private static LogicValue[][] ComputeBusMergeOutputs(LogicBusMergeNodeViewModel merge)
    {
        if (merge.OutputPins.Count == 0)
        {
            return Array.Empty<LogicValue[]>();
        }

        var width = Math.Max(1, merge.BitCount);
        var output = new LogicValue[width];
        for (var i = 0; i < width; i++)
        {
            if (i < merge.InputPins.Count)
            {
                output[i] = merge.InputPins[i].Value;
            }
            else
            {
                output[i] = LogicValue.Unknown;
            }
        }

        return new[] { output };
    }

    private LogicValue[] ComputeGateOutputs(LogicGateNodeViewModel gate, ISet<string> messages)
    {
        if (string.IsNullOrWhiteSpace(gate.ComponentId))
        {
            return CreateUnknownOutputs(gate.OutputPins.Count);
        }

        if (gate.InputPins.Count > 0 && gate.InputPins.Any(pin => pin.BusWidth > 1) ||
            gate.OutputPins.Count > 0 && gate.OutputPins.Any(pin => pin.BusWidth > 1))
        {
            messages.Add($"Bus pins are not supported on {gate.Title}.");
            return CreateUnknownOutputs(gate.OutputPins.Count);
        }

        var definition = _library.Get(gate.ComponentId);
        if (definition is null)
        {
            messages.Add($"Missing component definition: {gate.ComponentId}");
            return CreateUnknownOutputs(gate.OutputPins.Count);
        }

        var expectedInputs = definition.Inputs.Count;
        var actualInputs = gate.InputPins.Count;
        var inputCount = Math.Max(actualInputs, expectedInputs);
        var inputs = new LogicValue[inputCount];
        for (var i = 0; i < inputCount; i++)
        {
            inputs[i] = i < actualInputs ? gate.InputPins[i].Value : LogicValue.Unknown;
        }

        if (actualInputs < expectedInputs)
        {
            var componentId = string.IsNullOrWhiteSpace(gate.ComponentId) ? "unknown" : gate.ComponentId;
            messages.Add($"Pin mismatch on {gate.Title} ({componentId}): expected {expectedInputs} input(s), found {actualInputs}.");
        }

        var outputs = definition.Evaluate(inputs);
        if (outputs.Length == gate.OutputPins.Count)
        {
            return outputs;
        }

        var merged = new LogicValue[gate.OutputPins.Count];
        for (var i = 0; i < merged.Length; i++)
        {
            merged[i] = i < outputs.Length ? outputs[i] : LogicValue.Unknown;
        }

        return merged;
    }

    private static LogicValue[] GetFlipFlopOutputs(LogicFlipFlopNodeViewModel flipFlop)
    {
        if (flipFlop.OutputPins.Count == 0)
        {
            return Array.Empty<LogicValue>();
        }

        if (flipFlop.OutputPins.Count == 1)
        {
            return new[] { flipFlop.StoredValue };
        }

        return new[] { flipFlop.StoredValue, flipFlop.StoredValue.Not() };
    }

    private static LogicValue[] CreateUnknownOutputs(int count)
    {
        if (count <= 0)
        {
            return Array.Empty<LogicValue>();
        }

        var outputs = new LogicValue[count];
        for (var i = 0; i < count; i++)
        {
            outputs[i] = LogicValue.Unknown;
        }

        return outputs;
    }

    private static LogicValue[] CreateUniformOutputs(int count, LogicValue value)
    {
        if (count <= 0)
        {
            return Array.Empty<LogicValue>();
        }

        var outputs = new LogicValue[count];
        for (var i = 0; i < count; i++)
        {
            outputs[i] = value;
        }

        return outputs;
    }

    private static void UpdateInputPins(IDrawingNode drawing, LogicNodeContentViewModel content, ISet<string> messages, ref bool changed)
    {
        foreach (var pin in content.InputPins)
        {
            var signal = ResolveInputSignal(drawing, pin, messages);
            UpdatePinSignal(pin, signal, ref changed);
        }
    }

    private static LogicValue[] ResolveInputSignal(IDrawingNode drawing, LogicPinViewModel target, ISet<string> messages)
    {
        var width = Math.Max(1, target.BusWidth);
        if (drawing.Connectors is null)
        {
            return LogicSignalHelper.CreateUnknownBus(width);
        }

        var drivers = new List<(IConnector Connector, LogicPinViewModel OutputPin, LogicValue[] Signal)>();

        foreach (var connector in drawing.Connectors)
        {
            if (!TryGetConnectedPin(connector, target, out var other))
            {
                continue;
            }

            if (other is null || other.Parent is null)
            {
                continue;
            }

            if (!TryResolveConnection(connector, out var outputPin, out var inputPin, out var reason))
            {
                if (connector is LogicConnectorViewModel logicConnector)
                {
                    logicConnector.IsInvalid = true;
                    logicConnector.StatusMessage = reason;
                }

                continue;
            }

            if (!ReferenceEquals(inputPin, target))
            {
                continue;
            }

            var signal = GetPinSignal(outputPin);
            if (signal.Length != width)
            {
                if (connector is LogicConnectorViewModel logicMismatch)
                {
                    logicMismatch.IsInvalid = true;
                    logicMismatch.StatusMessage = "Bus width mismatch.";
                }

                continue;
            }

            drivers.Add((connector, outputPin, signal));
        }

        if (drivers.Count == 0)
        {
            return LogicSignalHelper.CreateUnknownBus(width);
        }

        if (drivers.Count == 1)
        {
            return drivers[0].Signal;
        }

        var result = new LogicValue[width];
        var hasContention = false;

        for (var bit = 0; bit < width; bit++)
        {
            var hasHigh = false;
            var hasLow = false;
            var hasUnknown = false;

            foreach (var driver in drivers)
            {
                var value = driver.Signal[bit];
                switch (value)
                {
                    case LogicValue.High:
                        hasHigh = true;
                        break;
                    case LogicValue.Low:
                        hasLow = true;
                        break;
                    default:
                        hasUnknown = true;
                        break;
                }
            }

            if (hasHigh && hasLow)
            {
                hasContention = true;
                result[bit] = LogicValue.Unknown;
            }
            else if (hasUnknown)
            {
                result[bit] = LogicValue.Unknown;
            }
            else if (hasHigh)
            {
                result[bit] = LogicValue.High;
            }
            else if (hasLow)
            {
                result[bit] = LogicValue.Low;
            }
            else
            {
                result[bit] = LogicValue.Unknown;
            }
        }

        if (hasContention)
        {
            messages.Add($"Signal contention on {target.Name}.");
            foreach (var driver in drivers)
            {
                if (driver.Connector is LogicConnectorViewModel logicConnector)
                {
                    logicConnector.IsContention = true;
                    logicConnector.StatusMessage = $"Contention on {target.Name}.";
                }
            }
        }

        return result;
    }

    private static bool TryGetConnectedPin(IConnector connector, LogicPinViewModel target, out LogicPinViewModel? other)
    {
        other = null;

        if (connector.Start == target && connector.End is LogicPinViewModel endPin)
        {
            other = endPin;
            return true;
        }

        if (connector.End == target && connector.Start is LogicPinViewModel startPin)
        {
            other = startPin;
            return true;
        }

        return false;
    }

    private static bool TryResolveConnection(IConnector connector, out LogicPinViewModel outputPin, out LogicPinViewModel inputPin, out string? reason)
    {
        outputPin = null!;
        inputPin = null!;
        reason = null;

        if (connector.Start is not LogicPinViewModel start || connector.End is not LogicPinViewModel end)
        {
            reason = "Connector is not fully connected.";
            return false;
        }

        if (start.Kind == LogicPinKind.Output && end.Kind == LogicPinKind.Input)
        {
            outputPin = start;
            inputPin = end;
        }
        else if (start.Kind == LogicPinKind.Input && end.Kind == LogicPinKind.Output)
        {
            outputPin = end;
            inputPin = start;
        }
        else
        {
            reason = "Invalid connector direction.";
            return false;
        }

        if (outputPin.BusWidth != inputPin.BusWidth)
        {
            reason = "Bus width mismatch.";
            return false;
        }

        return true;
    }

    private NodeTimingState GetTimingState(LogicNodeContentViewModel content)
    {
        if (!_timingStates.TryGetValue(content, out var state))
        {
            state = new NodeTimingState();
            _timingStates[content] = state;
        }

        return state;
    }

    private static bool OutputsEqual(IReadOnlyList<LogicPinViewModel> pins, IReadOnlyList<LogicValue[]> outputs)
    {
        if (pins.Count != outputs.Count)
        {
            return false;
        }

        for (var i = 0; i < outputs.Count; i++)
        {
            if (!SignalsEqual(GetPinSignal(pins[i]), outputs[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool OutputsEqual(IReadOnlyList<LogicValue[]> left, IReadOnlyList<LogicValue[]> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
        {
            if (!SignalsEqual(left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static void ApplyOutputs(LogicNodeContentViewModel content, IReadOnlyList<LogicValue[]> outputs, ref bool changed)
    {
        for (var i = 0; i < content.OutputPins.Count; i++)
        {
            var nextValue = i < outputs.Count ? outputs[i] : LogicSignalHelper.CreateUnknownBus(content.OutputPins[i].BusWidth);
            UpdatePinSignal(content.OutputPins[i], nextValue, ref changed);
        }
    }

    private static void UpdatePinValue(LogicPinViewModel pin, LogicValue value, ref bool changed)
    {
        if (pin.Value != value)
        {
            pin.Value = value;
            changed = true;
        }
    }

    private static void UpdatePinSignal(LogicPinViewModel pin, LogicValue[] signal, ref bool changed)
    {
        if (pin.BusWidth > 1)
        {
            var normalized = NormalizeSignal(signal, pin.BusWidth);
            if (!SignalsEqual(pin.BusValue, normalized))
            {
                pin.BusValue = normalized;
                changed = true;
            }

            return;
        }

        var value = signal.Length > 0 ? signal[0] : LogicValue.Unknown;
        UpdatePinValue(pin, value, ref changed);
    }

    private static LogicValue[] NormalizeSignal(IReadOnlyList<LogicValue> signal, int width)
    {
        var clampedWidth = Math.Max(1, width);
        if (signal.Count == clampedWidth)
        {
            return signal as LogicValue[] ?? signal.ToArray();
        }

        var normalized = new LogicValue[clampedWidth];
        for (var i = 0; i < normalized.Length; i++)
        {
            normalized[i] = i < signal.Count ? signal[i] : LogicValue.Unknown;
        }

        return normalized;
    }

    private static bool SignalsEqual(IReadOnlyList<LogicValue> left, IReadOnlyList<LogicValue> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }

    private static LogicValue[] GetPinSignal(LogicPinViewModel pin)
    {
        return pin.BusWidth > 1 ? pin.BusValue : new[] { pin.Value };
    }
}
