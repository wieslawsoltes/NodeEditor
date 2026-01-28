using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using NodeEditor.Model;

namespace NodeEditor;

internal static class HitTestHelper
{
    private const double IntersectionEpsilon = 1e-6;
    private static readonly ConditionalWeakTable<IDrawingNode, HitTestIndex> HitTestIndices = new();

    internal enum SelectionMode
    {
        Replace,
        Add,
        Toggle
    }

    public static double Length(Point pt0, Point pt1)
    {
        return Math.Sqrt(Math.Pow(pt1.X - pt0.X, 2) + Math.Pow(pt1.Y - pt0.Y, 2));
    }
        
    public static Point[] FlattenCubic(Point pt0, Point pt1, Point pt2, Point pt3)
    {
        var count = (int) Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2) + Length(pt2, pt3));
        var points = new Point[count];

        for (var i = 0; i < count; i++)
        {
            var t = (i + 1d) / count;
            var x = (1 - t) * (1 - t) * (1 - t) * pt0.X +
                    3 * t * (1 - t) * (1 - t) * pt1.X +
                    3 * t * t * (1 - t) * pt2.X +
                    t * t * t * pt3.X;
            var y = (1 - t) * (1 - t) * (1 - t) * pt0.Y +
                    3 * t * (1 - t) * (1 - t) * pt1.Y +
                    3 * t * t * (1 - t) * pt2.Y +
                    t * t * t * pt3.Y;
            points[i] = new Point(x, y);
        }

        return points;
    }

    public static bool HitTestConnector(IConnector connector, Rect rect)
    {
        if (!ConnectorPathHelper.TryGetEndpoints(connector, out var start, out var end))
        {
            return false;
        }

        var points = ConnectorPathHelper.GetFlattenedPath(connector, start, end);
        if (points.Count == 0)
        {
            return false;
        }

        if (points.Count == 1)
        {
            return rect.Contains(points[0]);
        }

        for (var i = 1; i < points.Count; i++)
        {
            if (SegmentIntersectsRect(points[i - 1], points[i], rect))
            {
                return true;
            }
        }

        return false;
    }

    public static Rect GetConnectorBounds(IConnector connector)
    {
        if (!ConnectorPathHelper.TryGetEndpoints(connector, out var start, out var end))
        {
            return default;
        }

        var points = ConnectorPathHelper.GetFlattenedPath(connector, start, end);
        if (points.Count == 0)
        {
            return default;
        }

        var topLeftX = points[0].X;
        var topLeftY = points[0].Y;
        var bottomRightX = points[0].X;
        var bottomRightY = points[0].Y;

        for (var i = 1; i < points.Count; i++)
        {
            topLeftX = Math.Min(topLeftX, points[i].X);
            topLeftY = Math.Min(topLeftY, points[i].Y);
            bottomRightX = Math.Max(bottomRightX, points[i].X);
            bottomRightY = Math.Max(bottomRightY, points[i].Y);
        }

        return new Rect(new Point(topLeftX, topLeftY), new Point(bottomRightX, bottomRightY));
    }

    public static bool TryFindConnectorAtPoint(IDrawingNode drawingNode, Point point, double tolerance, out IConnector? connector, out int segmentIndex)
    {
        connector = null;
        segmentIndex = -1;

        var index = GetIndex(drawingNode);
        var queryRect = new Rect(point.X - tolerance, point.Y - tolerance, tolerance * 2.0, tolerance * 2.0);
        var bestDistance = tolerance;

        foreach (var (candidate, candidateSegment, start, end) in index.QueryConnectorSegments(queryRect))
        {
            var distance = DistanceToSegment(point, start, end);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                connector = candidate;
                segmentIndex = candidateSegment;
            }
        }

        return connector is not null;
    }

    public static double DistanceToSegment(Point point, Point a, Point b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;

        if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
        {
            return Length(point, a);
        }

        var t = ((point.X - a.X) * dx + (point.Y - a.Y) * dy) / (dx * dx + dy * dy);
        t = Clamp(t, 0.0, 1.0);

        var closest = new Point(a.X + t * dx, a.Y + t * dy);
        return Length(point, closest);
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }

    private static bool SegmentIntersectsRect(Point a, Point b, Rect rect)
    {
        if (rect.Contains(a) || rect.Contains(b))
        {
            return true;
        }

        var minX = Math.Min(a.X, b.X);
        var maxX = Math.Max(a.X, b.X);
        var minY = Math.Min(a.Y, b.Y);
        var maxY = Math.Max(a.Y, b.Y);
        var segmentBounds = new Rect(new Point(minX, minY), new Point(maxX, maxY));

        if (!rect.Intersects(segmentBounds))
        {
            return false;
        }

        var topLeft = new Point(rect.Left, rect.Top);
        var topRight = new Point(rect.Right, rect.Top);
        var bottomLeft = new Point(rect.Left, rect.Bottom);
        var bottomRight = new Point(rect.Right, rect.Bottom);

        return SegmentsIntersect(a, b, topLeft, topRight)
               || SegmentsIntersect(a, b, topRight, bottomRight)
               || SegmentsIntersect(a, b, bottomRight, bottomLeft)
               || SegmentsIntersect(a, b, bottomLeft, topLeft);
    }

    private static bool SegmentsIntersect(Point a1, Point a2, Point b1, Point b2)
    {
        var d1 = Cross(a1, a2, b1);
        var d2 = Cross(a1, a2, b2);
        var d3 = Cross(b1, b2, a1);
        var d4 = Cross(b1, b2, a2);

        if (HasOppositeSigns(d1, d2) && HasOppositeSigns(d3, d4))
        {
            return true;
        }

        if (Math.Abs(d1) < IntersectionEpsilon && OnSegment(a1, a2, b1))
        {
            return true;
        }

        if (Math.Abs(d2) < IntersectionEpsilon && OnSegment(a1, a2, b2))
        {
            return true;
        }

        if (Math.Abs(d3) < IntersectionEpsilon && OnSegment(b1, b2, a1))
        {
            return true;
        }

        return Math.Abs(d4) < IntersectionEpsilon && OnSegment(b1, b2, a2);
    }

    private static bool HasOppositeSigns(double a, double b)
    {
        return (a > 0 && b < 0) || (a < 0 && b > 0);
    }

    private static double Cross(Point a, Point b, Point c)
    {
        return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    }

    private static bool OnSegment(Point a, Point b, Point p)
    {
        return p.X >= Math.Min(a.X, b.X) - IntersectionEpsilon
               && p.X <= Math.Max(a.X, b.X) + IntersectionEpsilon
               && p.Y >= Math.Min(a.Y, b.Y) - IntersectionEpsilon
               && p.Y <= Math.Max(a.Y, b.Y) + IntersectionEpsilon;
    }

    public static void FindSelectedNodes(IDrawingNode drawingNode, ItemsControl? itemsControl, Rect rect)
    {
        GetHitElements(drawingNode, itemsControl, rect, out var selectedNodes, out var selectedConnectors);
        ApplySelection(drawingNode, selectedNodes, selectedConnectors, SelectionMode.Replace);
    }

    public static void GetHitElements(
        IDrawingNode drawingNode,
        ItemsControl? itemsControl,
        Rect rect,
        out HashSet<INode> selectedNodes,
        out HashSet<IConnector> selectedConnectors)
    {
        selectedNodes = new HashSet<INode>();
        selectedConnectors = new HashSet<IConnector>();

        var index = GetIndex(drawingNode);

        if (drawingNode.CanSelectNodes())
        {
            foreach (var node in index.QueryNodes(rect))
            {
                if (node.CanSelect())
                {
                    selectedNodes.Add(node);
                }
            }
        }

        if (drawingNode.CanSelectConnectors())
        {
            foreach (var (connector, _, start, end) in index.QueryConnectorSegments(rect))
            {
                if (!SegmentIntersectsRect(start, end, rect))
                {
                    continue;
                }

                if (connector.CanSelect())
                {
                    selectedConnectors.Add(connector);
                }
            }
        }
    }

    public static bool ApplySelection(
        IDrawingNode drawingNode,
        ISet<INode> selectedNodes,
        ISet<IConnector> selectedConnectors,
        SelectionMode mode)
    {
        var currentNodes = drawingNode.GetSelectedNodes();
        var currentConnectors = drawingNode.GetSelectedConnectors();

        var nodesChanged = UpdateSelection(
            currentNodes,
            selectedNodes,
            mode,
            node => node.OnSelected(),
            node => node.OnDeselected(),
            out var nextNodes);

        var connectorsChanged = UpdateSelection(
            currentConnectors,
            selectedConnectors,
            mode,
            connector => connector.OnSelected(),
            connector => connector.OnDeselected(),
            out var nextConnectors);

        if (nodesChanged || connectorsChanged)
        {
            drawingNode.SetSelectedNodes(nextNodes);
            drawingNode.SetSelectedConnectors(nextConnectors);
            drawingNode.NotifySelectionChanged();
            return true;
        }

        return false;
    }

    private static bool UpdateSelection<T>(
        ISet<T>? current,
        ISet<T> hits,
        SelectionMode mode,
        Action<T> onSelected,
        Action<T> onDeselected,
        out ISet<T>? result)
    {
        var changed = false;

        switch (mode)
        {
            case SelectionMode.Replace:
                if (current is { Count: > 0 })
                {
                    foreach (var item in current)
                    {
                        if (!hits.Contains(item))
                        {
                            onDeselected(item);
                            changed = true;
                        }
                    }
                }

                foreach (var item in hits)
                {
                    if (current is null || !current.Contains(item))
                    {
                        onSelected(item);
                        changed = true;
                    }
                }

                result = hits.Count > 0 ? new HashSet<T>(hits) : null;
                break;
            case SelectionMode.Add:
                var added = current is not null ? new HashSet<T>(current) : new HashSet<T>();
                foreach (var item in hits)
                {
                    if (added.Add(item))
                    {
                        onSelected(item);
                        changed = true;
                    }
                }

                result = added.Count > 0 ? added : null;
                break;
            case SelectionMode.Toggle:
                var toggled = current is not null ? new HashSet<T>(current) : new HashSet<T>();
                foreach (var item in hits)
                {
                    if (toggled.Remove(item))
                    {
                        onDeselected(item);
                        changed = true;
                    }
                    else
                    {
                        toggled.Add(item);
                        onSelected(item);
                        changed = true;
                    }
                }

                result = toggled.Count > 0 ? toggled : null;
                break;
            default:
                result = current;
                break;
        }

        return changed;
    }

    public static Rect CalculateSelectedRect(IDrawingNode drawingNode, ItemsControl? itemsControl)
    {
        var selectedRect = new Rect();
        
        if (itemsControl is not null)
        {
            itemsControl.UpdateLayout();
        }

        var selectedNodes = drawingNode.GetSelectedNodes();

        if (selectedNodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                var bounds = GetNodeBounds(node);
                selectedRect = selectedRect == default ? bounds : selectedRect.Union(bounds);
            }
        }

        return selectedRect;
    }

    public static bool TryFindPinAtPoint(IDrawingNode drawingNode, Point point, double tolerance, out IPin? pin)
    {
        pin = null;

        if (drawingNode.Nodes is not { Count: > 0 })
        {
            return false;
        }

        foreach (var node in drawingNode.Nodes)
        {
            if (node.Pins is null || node.Pins.Count == 0)
            {
                continue;
            }

            var nodeX = node.X;
            var nodeY = node.Y;

            foreach (var candidate in node.Pins)
            {
                var x = nodeX + candidate.X;
                var y = nodeY + candidate.Y;
                var rect = new Rect(
                    x - tolerance,
                    y - tolerance,
                    candidate.Width + tolerance * 2,
                    candidate.Height + tolerance * 2);

                if (rect.Contains(point))
                {
                    pin = candidate;
                    return true;
                }
            }
        }

        return false;
    }

    internal static Rect GetNodeBounds(INode node)
    {
        var width = Math.Max(0.0, node.Width);
        var height = Math.Max(0.0, node.Height);
        var x = node.X;
        var y = node.Y;

        if (Math.Abs(node.Rotation) < 0.001 || width <= 0.0 || height <= 0.0)
        {
            return new Rect(x, y, width, height);
        }

        var centerX = x + width / 2.0;
        var centerY = y + height / 2.0;
        var radians = node.Rotation * Math.PI / 180.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);

        var points = new[]
        {
            new Point(x, y),
            new Point(x + width, y),
            new Point(x + width, y + height),
            new Point(x, y + height)
        };

        var minX = double.PositiveInfinity;
        var minY = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;
        var maxY = double.NegativeInfinity;

        foreach (var point in points)
        {
            var dx = point.X - centerX;
            var dy = point.Y - centerY;
            var rotatedX = dx * cos - dy * sin + centerX;
            var rotatedY = dx * sin + dy * cos + centerY;

            minX = Math.Min(minX, rotatedX);
            minY = Math.Min(minY, rotatedY);
            maxX = Math.Max(maxX, rotatedX);
            maxY = Math.Max(maxY, rotatedY);
        }

        return new Rect(new Point(minX, minY), new Point(maxX, maxY));
    }

    private static HitTestIndex GetIndex(IDrawingNode drawingNode)
    {
        return HitTestIndices.GetValue(drawingNode, node => new HitTestIndex(node));
    }
}
