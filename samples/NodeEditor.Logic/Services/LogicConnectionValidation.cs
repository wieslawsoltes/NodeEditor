using System;
using NodeEditor.Model;
using NodeEditorLogic.ViewModels;

namespace NodeEditorLogic.Services;

public static class LogicConnectionValidation
{
    public static bool TypeCompatibility(ConnectionValidationContext context)
    {
        if (context.Start is not LogicPinViewModel start || context.End is not LogicPinViewModel end)
        {
            return true;
        }

        var startType = ResolveType(start);
        var endType = ResolveType(end);

        return startType == endType;
    }

    private static LogicPinSignalType ResolveType(LogicPinViewModel pin)
    {
        if (pin.BusWidth > 1 || IsBusName(pin.Name))
        {
            return LogicPinSignalType.Bus;
        }

        if (IsClockName(pin.Name))
        {
            return LogicPinSignalType.Clock;
        }

        return LogicPinSignalType.Signal;
    }

    private static bool IsClockName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return name.Contains("clk", StringComparison.OrdinalIgnoreCase)
               || name.Contains("clock", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBusName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return name.Contains("bus", StringComparison.OrdinalIgnoreCase);
    }

    private enum LogicPinSignalType
    {
        Signal,
        Bus,
        Clock
    }
}
