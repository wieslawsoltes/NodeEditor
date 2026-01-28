using System;
using NodeEditor.Model;

namespace NodeEditorDemo.Services;

public static class BaseConnectionValidation
{
    public static bool TypeCompatibility(ConnectionValidationContext context)
    {
        if (context.Start is IConnectablePin start && context.End is IConnectablePin end)
        {
            if (!AreBusWidthsCompatible(start, end))
            {
                return false;
            }
        }

        var startType = ResolveType(context.Start.Name);
        var endType = ResolveType(context.End.Name);

        if (startType is null || endType is null)
        {
            return true;
        }

        return string.Equals(startType, endType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool AreBusWidthsCompatible(IConnectablePin start, IConnectablePin end)
    {
        var startWidth = Math.Max(1, start.BusWidth);
        var endWidth = Math.Max(1, end.BusWidth);

        if (startWidth == 1 && endWidth == 1)
        {
            return true;
        }

        return startWidth == endWidth;
    }

    private static string? ResolveType(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var index = name.IndexOf(':');
        if (index <= 0)
        {
            return null;
        }

        return name[..index];
    }
}
