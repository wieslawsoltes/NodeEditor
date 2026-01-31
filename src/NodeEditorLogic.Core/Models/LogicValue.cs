using System.Collections.Generic;

namespace NodeEditorLogic.Models;

public enum LogicValue
{
    Unknown,
    Low,
    High
}

public static class LogicValueExtensions
{
    public static LogicValue FromBool(bool value) => value ? LogicValue.High : LogicValue.Low;

    public static LogicValue FromNullableBool(bool? value)
    {
        if (value is null)
        {
            return LogicValue.Unknown;
        }

        return value.Value ? LogicValue.High : LogicValue.Low;
    }

    public static bool? ToNullableBool(this LogicValue value)
    {
        return value switch
        {
            LogicValue.High => true,
            LogicValue.Low => false,
            _ => null
        };
    }

    public static LogicValue Not(this LogicValue value)
    {
        return value switch
        {
            LogicValue.High => LogicValue.Low,
            LogicValue.Low => LogicValue.High,
            _ => LogicValue.Unknown
        };
    }

    public static LogicValue And(params LogicValue[] values)
    {
        if (values.Length == 0)
        {
            return LogicValue.Unknown;
        }

        var hasUnknown = false;

        foreach (var value in values)
        {
            if (value == LogicValue.Low)
            {
                return LogicValue.Low;
            }

            if (value == LogicValue.Unknown)
            {
                hasUnknown = true;
            }
        }

        return hasUnknown ? LogicValue.Unknown : LogicValue.High;
    }

    public static LogicValue And(IReadOnlyList<LogicValue> values)
    {
        if (values.Count == 0)
        {
            return LogicValue.Unknown;
        }

        var hasUnknown = false;

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (value == LogicValue.Low)
            {
                return LogicValue.Low;
            }

            if (value == LogicValue.Unknown)
            {
                hasUnknown = true;
            }
        }

        return hasUnknown ? LogicValue.Unknown : LogicValue.High;
    }

    public static LogicValue Or(params LogicValue[] values)
    {
        if (values.Length == 0)
        {
            return LogicValue.Unknown;
        }

        var hasUnknown = false;

        foreach (var value in values)
        {
            if (value == LogicValue.High)
            {
                return LogicValue.High;
            }

            if (value == LogicValue.Unknown)
            {
                hasUnknown = true;
            }
        }

        return hasUnknown ? LogicValue.Unknown : LogicValue.Low;
    }

    public static LogicValue Or(IReadOnlyList<LogicValue> values)
    {
        if (values.Count == 0)
        {
            return LogicValue.Unknown;
        }

        var hasUnknown = false;

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (value == LogicValue.High)
            {
                return LogicValue.High;
            }

            if (value == LogicValue.Unknown)
            {
                hasUnknown = true;
            }
        }

        return hasUnknown ? LogicValue.Unknown : LogicValue.Low;
    }

    public static LogicValue Xor(params LogicValue[] values)
    {
        if (values.Length == 0)
        {
            return LogicValue.Unknown;
        }

        var hasUnknown = false;
        var highCount = 0;

        foreach (var value in values)
        {
            if (value == LogicValue.Unknown)
            {
                hasUnknown = true;
            }
            else if (value == LogicValue.High)
            {
                highCount++;
            }
        }

        if (hasUnknown)
        {
            return LogicValue.Unknown;
        }

        return (highCount % 2 == 1) ? LogicValue.High : LogicValue.Low;
    }

    public static LogicValue Xor(IReadOnlyList<LogicValue> values)
    {
        if (values.Count == 0)
        {
            return LogicValue.Unknown;
        }

        var hasUnknown = false;
        var highCount = 0;

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (value == LogicValue.Unknown)
            {
                hasUnknown = true;
            }
            else if (value == LogicValue.High)
            {
                highCount++;
            }
        }

        if (hasUnknown)
        {
            return LogicValue.Unknown;
        }

        return (highCount % 2 == 1) ? LogicValue.High : LogicValue.Low;
    }

    public static LogicValue Mux(LogicValue a, LogicValue b, LogicValue select)
    {
        if (select == LogicValue.Unknown)
        {
            return LogicValue.Unknown;
        }

        return select == LogicValue.High ? b : a;
    }
}
