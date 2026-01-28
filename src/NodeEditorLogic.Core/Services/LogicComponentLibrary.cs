using System.Collections.Generic;
using NodeEditorLogic.Models;

namespace NodeEditorLogic.Services;

public sealed class LogicComponentLibrary
{
    private readonly Dictionary<string, LogicComponentDefinition> _definitions = new();

    public LogicComponentLibrary()
    {
        AddGate("and", "AND", "Gates", 1, new[] { "A", "B" }, new[] { "Q" },
            inputs => new[] { LogicValueExtensions.And(inputs) });

        AddGate("or", "OR", "Gates", 1, new[] { "A", "B" }, new[] { "Q" },
            inputs => new[] { LogicValueExtensions.Or(inputs) });

        AddGate("not", "NOT", "Gates", 1, new[] { "In" }, new[] { "Q" },
            inputs => new[] { inputs[0].Not() });

        AddGate("nand", "NAND", "Gates", 1, new[] { "A", "B" }, new[] { "Q" },
            inputs => new[] { LogicValueExtensions.And(inputs).Not() });

        AddGate("nor", "NOR", "Gates", 1, new[] { "A", "B" }, new[] { "Q" },
            inputs => new[] { LogicValueExtensions.Or(inputs).Not() });

        AddGate("xor", "XOR", "Gates", 1, new[] { "A", "B" }, new[] { "Q" },
            inputs => new[] { LogicValueExtensions.Xor(inputs) });

        AddGate("xnor", "XNOR", "Gates", 1, new[] { "A", "B" }, new[] { "Q" },
            inputs => new[] { LogicValueExtensions.Xor(inputs).Not() });

        AddComponent("half-adder", "Half Adder", "Components", 2, new[] { "A", "B" }, new[] { "Sum", "Carry" },
            inputs =>
            {
                var sum = LogicValueExtensions.Xor(inputs[0], inputs[1]);
                var carry = LogicValueExtensions.And(inputs[0], inputs[1]);
                return new[] { sum, carry };
            });

        AddComponent("full-adder", "Full Adder", "Components", 2, new[] { "A", "B", "Cin" }, new[] { "Sum", "Cout" },
            inputs =>
            {
                var ab = LogicValueExtensions.Xor(inputs[0], inputs[1]);
                var sum = LogicValueExtensions.Xor(ab, inputs[2]);
                var carry = LogicValueExtensions.Or(
                    LogicValueExtensions.And(inputs[0], inputs[1]),
                    LogicValueExtensions.And(inputs[2], ab));
                return new[] { sum, carry };
            });

        AddComponent("mux2", "2:1 Mux", "Components", 1, new[] { "A", "B", "Sel" }, new[] { "Out" },
            inputs => new[] { LogicValueExtensions.Mux(inputs[0], inputs[1], inputs[2]) });

        AddComponent("decoder2", "2:4 Decoder", "Components", 2, new[] { "A", "B" }, new[] { "D0", "D1", "D2", "D3" },
            inputs =>
            {
                if (inputs[0] == LogicValue.Unknown || inputs[1] == LogicValue.Unknown)
                {
                    return new[] { LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown };
                }

                var a = inputs[0] == LogicValue.High;
                var b = inputs[1] == LogicValue.High;
                return new[]
                {
                    (!a && !b) ? LogicValue.High : LogicValue.Low,
                    (!a && b) ? LogicValue.High : LogicValue.Low,
                    (a && !b) ? LogicValue.High : LogicValue.Low,
                    (a && b) ? LogicValue.High : LogicValue.Low
                };
            });

        AddIc("ic-7400", "74HC00 Quad NAND", 2,
            new[] { "A1", "B1", "A2", "B2", "A3", "B3", "A4", "B4" },
            new[] { "Y1", "Y2", "Y3", "Y4" },
            inputs => new[]
            {
                LogicValueExtensions.And(inputs[0], inputs[1]).Not(),
                LogicValueExtensions.And(inputs[2], inputs[3]).Not(),
                LogicValueExtensions.And(inputs[4], inputs[5]).Not(),
                LogicValueExtensions.And(inputs[6], inputs[7]).Not()
            });

        AddIc("ic-7408", "74HC08 Quad AND", 2,
            new[] { "A1", "B1", "A2", "B2", "A3", "B3", "A4", "B4" },
            new[] { "Y1", "Y2", "Y3", "Y4" },
            inputs => new[]
            {
                LogicValueExtensions.And(inputs[0], inputs[1]),
                LogicValueExtensions.And(inputs[2], inputs[3]),
                LogicValueExtensions.And(inputs[4], inputs[5]),
                LogicValueExtensions.And(inputs[6], inputs[7])
            });

        AddIc("ic-7432", "74HC32 Quad OR", 2,
            new[] { "A1", "B1", "A2", "B2", "A3", "B3", "A4", "B4" },
            new[] { "Y1", "Y2", "Y3", "Y4" },
            inputs => new[]
            {
                LogicValueExtensions.Or(inputs[0], inputs[1]),
                LogicValueExtensions.Or(inputs[2], inputs[3]),
                LogicValueExtensions.Or(inputs[4], inputs[5]),
                LogicValueExtensions.Or(inputs[6], inputs[7])
            });

        AddIc("ic-7486", "74HC86 Quad XOR", 2,
            new[] { "A1", "B1", "A2", "B2", "A3", "B3", "A4", "B4" },
            new[] { "Y1", "Y2", "Y3", "Y4" },
            inputs => new[]
            {
                LogicValueExtensions.Xor(inputs[0], inputs[1]),
                LogicValueExtensions.Xor(inputs[2], inputs[3]),
                LogicValueExtensions.Xor(inputs[4], inputs[5]),
                LogicValueExtensions.Xor(inputs[6], inputs[7])
            });

        AddIc("ic-7404", "74HC04 Hex Inverter", 2,
            new[] { "A1", "A2", "A3", "A4", "A5", "A6" },
            new[] { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6" },
            inputs => new[]
            {
                inputs[0].Not(),
                inputs[1].Not(),
                inputs[2].Not(),
                inputs[3].Not(),
                inputs[4].Not(),
                inputs[5].Not()
            });

        AddIc("ic-74157", "74HC157 Quad 2:1 Mux", 2,
            new[] { "A1", "A2", "A3", "A4", "B1", "B2", "B3", "B4", "Sel" },
            new[] { "Y1", "Y2", "Y3", "Y4" },
            inputs => new[]
            {
                LogicValueExtensions.Mux(inputs[0], inputs[4], inputs[8]),
                LogicValueExtensions.Mux(inputs[1], inputs[5], inputs[8]),
                LogicValueExtensions.Mux(inputs[2], inputs[6], inputs[8]),
                LogicValueExtensions.Mux(inputs[3], inputs[7], inputs[8])
            });

        AddIc("ic-74138", "74HC138 3:8 Decoder", 3,
            new[] { "A", "B", "C" },
            new[] { "Y0", "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7" },
            inputs =>
            {
                if (inputs[0] == LogicValue.Unknown || inputs[1] == LogicValue.Unknown || inputs[2] == LogicValue.Unknown)
                {
                    return new[]
                    {
                        LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown,
                        LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown
                    };
                }

                var index = (inputs[2] == LogicValue.High ? 4 : 0)
                            + (inputs[1] == LogicValue.High ? 2 : 0)
                            + (inputs[0] == LogicValue.High ? 1 : 0);

                var outputs = new LogicValue[8];
                for (var i = 0; i < outputs.Length; i++)
                {
                    outputs[i] = i == index ? LogicValue.High : LogicValue.Low;
                }

                return outputs;
            });

        AddIc("ic-74283", "74HC283 4-bit Adder", 3,
            new[] { "A0", "A1", "A2", "A3", "B0", "B1", "B2", "B3", "Cin" },
            new[] { "S0", "S1", "S2", "S3", "Cout" },
            inputs =>
            {
                if (HasUnknown(inputs, 9))
                {
                    return new[] { LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown, LogicValue.Unknown };
                }

                var a = BitsToInt(inputs, 0, 4);
                var b = BitsToInt(inputs, 4, 4);
                var cin = inputs[8] == LogicValue.High ? 1 : 0;
                var sum = a + b + cin;

                return new[]
                {
                    IntToBit(sum, 0),
                    IntToBit(sum, 1),
                    IntToBit(sum, 2),
                    IntToBit(sum, 3),
                    (sum > 15) ? LogicValue.High : LogicValue.Low
                };
            });
    }

    public IReadOnlyCollection<LogicComponentDefinition> Definitions => _definitions.Values;

    public LogicComponentDefinition? Get(string id)
    {
        return _definitions.TryGetValue(id, out var definition) ? definition : null;
    }

    private void AddGate(
        string id,
        string title,
        string category,
        int propagationDelay,
        IReadOnlyList<string> inputs,
        IReadOnlyList<string> outputs,
        System.Func<IReadOnlyList<LogicValue>, LogicValue[]> evaluate)
    {
        _definitions[id] = new LogicComponentDefinition(id, title, category, propagationDelay, inputs, outputs, evaluate);
    }

    private void AddComponent(
        string id,
        string title,
        string category,
        int propagationDelay,
        IReadOnlyList<string> inputs,
        IReadOnlyList<string> outputs,
        System.Func<IReadOnlyList<LogicValue>, LogicValue[]> evaluate)
    {
        _definitions[id] = new LogicComponentDefinition(id, title, category, propagationDelay, inputs, outputs, evaluate);
    }

    private void AddIc(
        string id,
        string title,
        int propagationDelay,
        IReadOnlyList<string> inputs,
        IReadOnlyList<string> outputs,
        System.Func<IReadOnlyList<LogicValue>, LogicValue[]> evaluate)
    {
        _definitions[id] = new LogicComponentDefinition(id, title, "IC Library", propagationDelay, inputs, outputs, evaluate);
    }

    private static bool HasUnknown(IReadOnlyList<LogicValue> inputs, int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (inputs[i] == LogicValue.Unknown)
            {
                return true;
            }
        }

        return false;
    }

    private static int BitsToInt(IReadOnlyList<LogicValue> inputs, int offset, int width)
    {
        var value = 0;
        for (var i = 0; i < width; i++)
        {
            if (inputs[offset + i] == LogicValue.High)
            {
                value |= 1 << i;
            }
        }

        return value;
    }

    private static LogicValue IntToBit(int value, int bit)
    {
        return (value & (1 << bit)) != 0 ? LogicValue.High : LogicValue.Low;
    }
}
