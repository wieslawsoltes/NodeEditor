using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace NodeEditor;

/// <summary>A common path representation ensures rendering and interaction use identical segments.</summary>
internal sealed class ConnectorPath
{
    private readonly Point _start;
    private readonly List<Segment> _segments = new();
    private Point _current;

    internal ConnectorPath(Point start)
    {
        _start = start;
        _current = start;
    }

    internal void LineTo(Point point)
    {
        if (((Vector)(_current - point)).Length > 0.000001)
        {
            _segments.Add(new Segment(_current, default, default, point, false));
        }
        _current = point;
    }

    internal void CubicTo(Point control1, Point control2, Point end)
    {
        _segments.Add(new Segment(_current, control1, control2, end, true));
        _current = end;
    }

    internal StreamGeometry CreateGeometry()
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(_start, false);
            foreach (var segment in _segments)
            {
                if (segment.IsCubic)
                {
                    context.CubicBezierTo(segment.Control1, segment.Control2, segment.End);
                }
                else
                {
                    context.LineTo(segment.End);
                }
            }
            context.EndFigure(false);
        }
        return geometry;
    }

    internal List<Point> Flatten(double tolerance)
    {
        var points = new List<Point>(_segments.Count * 4 + 1) { _start };
        var squaredTolerance = tolerance * tolerance;
        foreach (var segment in _segments)
        {
            if (segment.IsCubic)
            {
                FlattenCubic(points, segment.Start, segment.Control1, segment.Control2, segment.End, squaredTolerance, 0);
            }
            else
            {
                points.Add(segment.End);
            }
        }
        return points;
    }

    private static void FlattenCubic(List<Point> points, Point p0, Point p1, Point p2, Point p3, double toleranceSquared, int depth)
    {
        if (depth >= 12 || (DistanceSquaredToSegment(p1, p0, p3) <= toleranceSquared
                           && DistanceSquaredToSegment(p2, p0, p3) <= toleranceSquared))
        {
            points.Add(p3);
            return;
        }

        var a = Midpoint(p0, p1);
        var b = Midpoint(p1, p2);
        var c = Midpoint(p2, p3);
        var d = Midpoint(a, b);
        var e = Midpoint(b, c);
        var middle = Midpoint(d, e);
        FlattenCubic(points, p0, a, d, middle, toleranceSquared, depth + 1);
        FlattenCubic(points, middle, e, c, p3, toleranceSquared, depth + 1);
    }

    private static Point Midpoint(Point a, Point b) => new((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5);

    private static double DistanceSquaredToSegment(Point point, Point start, Point end)
    {
        var delta = end - start;
        var lengthSquared = delta.X * delta.X + delta.Y * delta.Y;
        var relative = point - start;
        if (lengthSquared <= 0.000001)
        {
            return relative.X * relative.X + relative.Y * relative.Y;
        }

        var fraction = Math.Clamp((relative.X * delta.X + relative.Y * delta.Y) / lengthSquared, 0, 1);
        var distance = point - (start + delta * fraction);
        return distance.X * distance.X + distance.Y * distance.Y;
    }

    private readonly record struct Segment(Point Start, Point Control1, Point Control2, Point End, bool IsCubic);
}
