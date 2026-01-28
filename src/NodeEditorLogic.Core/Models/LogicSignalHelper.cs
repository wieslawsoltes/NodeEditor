using System;
using System.Collections.Generic;

namespace NodeEditorLogic.Models;

public static class LogicSignalHelper
{
    public static LogicValue[] CreateBusFromInt(int value, int width)
    {
        var clampedWidth = Math.Max(1, width);
        var bits = new LogicValue[clampedWidth];
        for (var i = 0; i < clampedWidth; i++)
        {
            bits[i] = ((value >> i) & 1) == 1 ? LogicValue.High : LogicValue.Low;
        }

        return bits;
    }

    public static LogicValue[] CreateUnknownBus(int width)
    {
        var clampedWidth = Math.Max(1, width);
        var bits = new LogicValue[clampedWidth];
        for (var i = 0; i < bits.Length; i++)
        {
            bits[i] = LogicValue.Unknown;
        }

        return bits;
    }

    public static int? ToInt(IReadOnlyList<LogicValue> bits)
    {
        var value = 0;
        for (var i = 0; i < bits.Count; i++)
        {
            var bit = bits[i];
            if (bit == LogicValue.Unknown)
            {
                return null;
            }

            if (bit == LogicValue.High)
            {
                value |= 1 << i;
            }
        }

        return value;
    }

    public static LogicValue Aggregate(IReadOnlyList<LogicValue> bits)
    {
        var hasHigh = false;
        var hasLow = false;

        for (var i = 0; i < bits.Count; i++)
        {
            switch (bits[i])
            {
                case LogicValue.High:
                    hasHigh = true;
                    break;
                case LogicValue.Low:
                    hasLow = true;
                    break;
                default:
                    return LogicValue.Unknown;
            }
        }

        if (hasHigh && !hasLow)
        {
            return LogicValue.High;
        }

        if (hasLow && !hasHigh)
        {
            return LogicValue.Low;
        }

        return LogicValue.Unknown;
    }

    public static string ToBinaryString(IReadOnlyList<LogicValue> bits)
    {
        if (bits.Count == 0)
        {
            return "";
        }

        var chars = new char[bits.Count];
        for (var i = 0; i < bits.Count; i++)
        {
            var bit = bits[bits.Count - 1 - i];
            chars[i] = bit switch
            {
                LogicValue.High => '1',
                LogicValue.Low => '0',
                _ => 'X'
            };
        }

        return "0b" + new string(chars);
    }

    public static string ToHexString(IReadOnlyList<LogicValue> bits)
    {
        var value = ToInt(bits);
        if (value is null)
        {
            return ToBinaryString(bits);
        }

        var width = Math.Max(1, bits.Count);
        var digits = (int)Math.Ceiling(width / 4.0);
        return "0x" + value.Value.ToString("X" + digits);
    }
}
