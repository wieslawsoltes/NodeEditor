using System;
using Avalonia;

namespace NodeEditor;

internal static class SnapHelper
{
    public static double Snap(double value, double snap)
    {
        var step = Math.Abs(snap);
        if (step <= 0.0)
        {
            return value;
        }

        return Math.Round(value / step, MidpointRounding.AwayFromZero) * step;
    }

    public static Point Snap(Point point, double snapX, double snapY, bool enabled)
    {
        if (enabled)
        {
            var pointX = Snap(point.X, snapX);
            var pointY = Snap(point.Y, snapY);
            return new Point(pointX, pointY);
        }

        return point;
    }
}
