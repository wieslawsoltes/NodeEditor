using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using NodeEditor.Model;

namespace NodeEditor.Controls;

internal static class InkGeometryBuilder
{
    public static StreamGeometry? Build(IList<InkPoint> points)
    {
        if (points is null || points.Count == 0)
        {
            return null;
        }

        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            var start = new Point(points[0].X, points[0].Y);
            context.BeginFigure(start, false);

            for (var i = 1; i < points.Count; i++)
            {
                var point = points[i];
                context.LineTo(new Point(point.X, point.Y));
            }

            context.EndFigure(false);
        }

        return geometry;
    }
}
