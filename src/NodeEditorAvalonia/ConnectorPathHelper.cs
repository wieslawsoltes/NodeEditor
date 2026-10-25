using System;
using System.Collections.Generic;
using Avalonia;
using NodeEditor.Model;

namespace NodeEditor;

internal static class ConnectorPathHelper
{
    public static bool TryGetEndpoints(IConnector connector, out Point start, out Point end)
    {
        start = default;
        end = default;

        if (connector.Start is null || connector.End is null)
        {
            return false;
        }

        start = GetPinPoint(connector.Start);
        end = GetPinPoint(connector.End);
        return double.IsFinite(start.X) && double.IsFinite(start.Y)
            && double.IsFinite(end.X) && double.IsFinite(end.Y);
    }

    public static List<Point> GetPolylinePoints(IConnector connector, Point start, Point end)
    {
        if (ZipTieGeometry.TryCreatePath(connector, start, end, out var bundledPath))
        {
            return bundledPath.Flatten(0.35);
        }

        var settings = connector.Parent?.Settings;
        var routingEnabled = settings is not null && settings.EnableConnectorRouting;
        var manual = connector.RoutingMode == ConnectorRoutingMode.Manual;

        if (manual)
        {
            if (connector.Waypoints is { Count: > 0 })
            {
                return BuildWaypointPath(start, end, connector.Waypoints);
            }

            return BuildFallbackPath(connector, start, end);
        }

        if (routingEnabled && settings is not null)
        {
            var allowDiagonal = ShouldAllowDiagonal(settings, connector.Style);
            var preferDirect = connector.Style != ConnectorStyle.Orthogonal;

            if (OrthogonalRouter.TryRoute(connector, start, end, allowDiagonal, preferDirect, out var routed))
            {
                return routed;
            }
        }

        if (connector.Waypoints is { Count: > 0 })
        {
            return BuildWaypointPath(start, end, connector.Waypoints);
        }

        return BuildFallbackPath(connector, start, end);
    }

    public static List<Point> GetFlattenedPath(IConnector connector, Point start, Point end)
        => BuildPath(connector, start, end).Flatten(0.35);

    internal static ConnectorPath BuildPath(IConnector connector, Point start, Point end)
    {
        if (ZipTieGeometry.TryCreatePath(connector, start, end, out var bundledPath))
        {
            return bundledPath;
        }

        var routed = GetPolylinePoints(connector, start, end);
        var path = new ConnectorPath(start);
        if (connector.Style == ConnectorStyle.Bezier)
        {
            if (routed.Count <= 2 && connector.Waypoints is not { Count: > 0 })
            {
                var p1X = start.X;
                var p1Y = start.Y;
                var p2X = end.X;
                var p2Y = end.Y;
                connector.GetControlPoints(
                    connector.Orientation,
                    connector.Offset,
                    connector.Start?.Alignment ?? PinAlignment.None,
                    connector.End?.Alignment ?? PinAlignment.None,
                    ref p1X,
                    ref p1Y,
                    ref p2X,
                    ref p2Y);
                path.CubicTo(new Point(p1X, p1Y), new Point(p2X, p2Y), end);
                return path;
            }

            var radius = connector.Parent?.Settings.RoutingCornerRadius ?? 0;
            if (double.IsFinite(radius) && radius > 0.001)
            {
                AppendRoundedPolyline(path, routed, radius);
                return path;
            }
        }

        for (var i = 1; i < routed.Count; i++)
        {
            path.LineTo(routed[i]);
        }
        return path;
    }

    private static void AppendRoundedPolyline(ConnectorPath path, IReadOnlyList<Point> points, double radius)
    {
        for (var i = 1; i < points.Count - 1; i++)
        {
            var previous = points[i - 1];
            var current = points[i];
            var next = points[i + 1];
            Vector incoming = previous - current;
            Vector outgoing = next - current;
            var incomingLength = incoming.Length;
            var outgoingLength = outgoing.Length;
            var cross = incoming.X * outgoing.Y - incoming.Y * outgoing.X;
            if (incomingLength < 0.001 || outgoingLength < 0.001 || Math.Abs(cross) < 0.001)
            {
                path.LineTo(current);
                continue;
            }

            var distance = Math.Min(radius, Math.Min(incomingLength, outgoingLength) * 0.5);
            var curveStart = current + incoming * (distance / incomingLength);
            var curveEnd = current + outgoing * (distance / outgoingLength);
            path.LineTo(curveStart);
            path.CubicTo(current, current, curveEnd);
        }

        if (points.Count > 1)
        {
            path.LineTo(points[points.Count - 1]);
        }
    }

    public static Point GetLabelPoint(IConnector connector, Point start, Point end)
    {
        var points = GetFlattenedPath(connector, start, end);
        if (points.Count == 0)
        {
            return default;
        }

        if (points.Count == 1)
        {
            return points[0];
        }

        var totalLength = 0.0;
        for (var i = 1; i < points.Count; i++)
        {
            totalLength += Distance(points[i - 1], points[i]);
        }

        if (totalLength <= 0.001)
        {
            return points[points.Count / 2];
        }

        var target = totalLength / 2.0;
        var traveled = 0.0;

        for (var i = 1; i < points.Count; i++)
        {
            var segment = Distance(points[i - 1], points[i]);
            if (segment <= 0.0001)
            {
                continue;
            }

            if (traveled + segment >= target)
            {
                var t = (target - traveled) / segment;
                return new Point(
                    points[i - 1].X + (points[i].X - points[i - 1].X) * t,
                    points[i - 1].Y + (points[i].Y - points[i - 1].Y) * t);
            }

            traveled += segment;
        }

        return points[points.Count - 1];
    }

    internal static Point GetPinPoint(IPin pin)
    {
        var x = pin.X;
        var y = pin.Y;

        if (pin.Parent is not null)
        {
            if (Math.Abs(pin.Parent.Rotation) > 0.001)
            {
                var centerX = pin.Parent.Width / 2.0;
                var centerY = pin.Parent.Height / 2.0;
                var radians = pin.Parent.Rotation * Math.PI / 180.0;
                var cos = Math.Cos(radians);
                var sin = Math.Sin(radians);
                var dx = x - centerX;
                var dy = y - centerY;
                var rotatedX = dx * cos - dy * sin + centerX;
                var rotatedY = dx * sin + dy * cos + centerY;
                x = rotatedX;
                y = rotatedY;
            }

            x += pin.Parent.X;
            y += pin.Parent.Y;
        }

        return new Point(x, y);
    }

    private static ConnectorOrientation GetAutoOrientation(IConnector connector, Point start, Point end)
    {
        if (connector.Orientation != ConnectorOrientation.Auto)
        {
            return connector.Orientation;
        }

        var startAlignment = connector.Start?.Alignment ?? PinAlignment.None;
        var endAlignment = connector.End?.Alignment ?? PinAlignment.None;

        if (startAlignment is PinAlignment.Left or PinAlignment.Right)
        {
            return ConnectorOrientation.Horizontal;
        }

        if (startAlignment is PinAlignment.Top or PinAlignment.Bottom)
        {
            return ConnectorOrientation.Vertical;
        }

        if (endAlignment is PinAlignment.Left or PinAlignment.Right)
        {
            return ConnectorOrientation.Horizontal;
        }

        if (endAlignment is PinAlignment.Top or PinAlignment.Bottom)
        {
            return ConnectorOrientation.Vertical;
        }

        return Math.Abs(start.X - end.X) >= Math.Abs(start.Y - end.Y)
            ? ConnectorOrientation.Horizontal
            : ConnectorOrientation.Vertical;
    }

    private static List<Point> BuildWaypointPath(Point start, Point end, IList<ConnectorPoint> waypoints)
    {
        var points = new List<Point>(waypoints.Count + 2) { start };
        foreach (var waypoint in waypoints)
        {
            points.Add(new Point(waypoint.X, waypoint.Y));
        }

        points.Add(end);
        return points;
    }

    private static List<Point> BuildFallbackPath(IConnector connector, Point start, Point end)
    {
        var points = new List<Point> { start };

        if (connector.Style == ConnectorStyle.Orthogonal)
        {
            var orientation = GetAutoOrientation(connector, start, end);
            if (!IsAligned(start, end))
            {
                if (orientation == ConnectorOrientation.Horizontal)
                {
                    var midX = (start.X + end.X) / 2.0;
                    points.Add(new Point(midX, start.Y));
                    points.Add(new Point(midX, end.Y));
                }
                else
                {
                    var midY = (start.Y + end.Y) / 2.0;
                    points.Add(new Point(start.X, midY));
                    points.Add(new Point(end.X, midY));
                }
            }
        }

        points.Add(end);
        return points;
    }

    private static bool ShouldAllowDiagonal(IDrawingNodeSettings settings, ConnectorStyle style)
    {
        return settings.RoutingAlgorithm switch
        {
            ConnectorRoutingAlgorithm.Orthogonal => false,
            ConnectorRoutingAlgorithm.Octilinear => true,
            _ => style != ConnectorStyle.Orthogonal
        };
    }

    private static bool IsAligned(Point start, Point end)
    {
        return Math.Abs(start.X - end.X) < 0.001 || Math.Abs(start.Y - end.Y) < 0.001;
    }

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
