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
        return true;
    }

    public static List<Point> GetPolylinePoints(IConnector connector, Point start, Point end)
    {
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
    {
        if (connector.Style == ConnectorStyle.Bezier)
        {
            var routed = GetPolylinePoints(connector, start, end);
            var hasWaypoints = connector.Waypoints is { Count: > 0 };
            if (routed.Count > 2 || hasWaypoints)
            {
                return routed;
            }

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

            var pt0 = start;
            var pt1 = new Point(p1X, p1Y);
            var pt2 = new Point(p2X, p2Y);
            var pt3 = end;

            var flattened = HitTestHelper.FlattenCubic(pt0, pt1, pt2, pt3);
            var points = new List<Point>(flattened.Length + 1) { pt0 };
            points.AddRange(flattened);
            return points;
        }

        return GetPolylinePoints(connector, start, end);
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
