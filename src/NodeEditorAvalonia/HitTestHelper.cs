using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using NodeEditor.Model;

namespace NodeEditor;

internal static class HitTestHelper
{
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
        var start = connector.Start;
        var end = connector.End;
        if (start is null || end is null)
        {
            return false;
        }

        var p0X = start.X;
        var p0Y = start.Y;
        if (start.Parent is { })
        {
            p0X += start.Parent.X;
            p0Y += start.Parent.Y; 
        }

        var p3X = end.X;
        var p3Y = end.Y;
        if (end.Parent is { })
        {
            p3X += end.Parent.X;
            p3Y += end.Parent.Y; 
        }
            
        var p1X = p0X;
        var p1Y = p0Y;

        var p2X = p3X;
        var p2Y = p3Y;

        connector.GetControlPoints(
            connector.Orientation, 
            connector.Offset, 
            start.Alignment,
            end.Alignment,
            ref p1X, ref p1Y, 
            ref p2X, ref p2Y);

        var pt0 = new Point(p0X, p0Y);
        var pt1 = new Point(p1X, p1Y);
        var pt2 = new Point(p2X, p2Y);
        var pt3 = new Point(p3X, p3Y);

        var points = FlattenCubic(pt0, pt1, pt2, pt3);

        for (var i = 0; i < points.Length; i++)
        {
            if (rect.Contains(points[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static Rect GetConnectorBounds(IConnector connector)
    {
        var start = connector.Start;
        var end = connector.End;
        if (start is null || end is null)
        {
            return default;
        }

        var p0X = start.X;
        var p0Y = start.Y;
        if (start.Parent is { })
        {
            p0X += start.Parent.X;
            p0Y += start.Parent.Y; 
        }

        var p3X = end.X;
        var p3Y = end.Y;
        if (end.Parent is { })
        {
            p3X += end.Parent.X;
            p3Y += end.Parent.Y; 
        }

        var p1X = p0X;
        var p1Y = p0Y;

        var p2X = p3X;
        var p2Y = p3Y;

        connector.GetControlPoints(
            connector.Orientation, 
            connector.Offset, 
            start.Alignment,
            end.Alignment,
            ref p1X, ref p1Y, 
            ref p2X, ref p2Y);

        var pt0 = new Point(p0X, p0Y);
        var pt1 = new Point(p1X, p1Y);
        var pt2 = new Point(p2X, p2Y);
        var pt3 = new Point(p3X, p3Y);

        var points = FlattenCubic(pt0, pt1, pt2, pt3);

        var topLeftX = 0.0;
        var topLeftY = 0.0;
        var bottomRightX = 0.0;
        var bottomRightY = 0.0;

        for (var i = 0; i < points.Length; i++)
        {
            if (i == 0)
            {
                topLeftX = points[i].X;
                topLeftY = points[i].Y;
                bottomRightX = points[i].X;
                bottomRightY = points[i].Y;
            }
            else
            {
                topLeftX = Math.Min(topLeftX, points[i].X);
                topLeftY = Math.Min(topLeftY, points[i].Y);
                bottomRightX = Math.Max(bottomRightX, points[i].X);
                bottomRightY = Math.Max(bottomRightY, points[i].Y);
            }
        }

        return new Rect(
            new Point(topLeftX, topLeftY), 
            new Point(bottomRightX, bottomRightY));
    }

    public static void FindSelectedNodes(ItemsControl? itemsControl, Rect rect)
    {
        if (itemsControl?.DataContext is not IDrawingNode drawingNode)
        {
            return;
        }

        drawingNode.NotifyDeselectedNodes();
        drawingNode.NotifyDeselectedConnectors();
        drawingNode.SetSelectedNodes(null);
        drawingNode.SetSelectedConnectors(null);
        drawingNode.NotifySelectionChanged();

        var selectedNodes = new HashSet<INode>();
        var selectedConnectors = new HashSet<IConnector>();

        if (drawingNode.CanSelectNodes())
        {
            foreach (var control in itemsControl.GetRealizedContainers())
            {
                if (control is not { DataContext: INode node } containerControl)
                {
                    continue;
                }

                var bounds = containerControl.Bounds;

                if (!rect.Intersects(bounds))
                {
                    continue;
                }

                if (node.CanSelect())
                {
                    selectedNodes.Add(node);
                }
            }
        }

        if (drawingNode.CanSelectConnectors())
        {
            if (drawingNode.Connectors is { Count: > 0 })
            {
                foreach (var connector in drawingNode.Connectors)
                {
                    if (!HitTestConnector(connector, rect))
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

        var notify = false;

        if (selectedConnectors.Count > 0)
        {
            drawingNode.SetSelectedConnectors(selectedConnectors);
            notify = true;
        }

        if (selectedNodes.Count > 0)
        {
            drawingNode.SetSelectedNodes(selectedNodes);
            notify = true;
        }

        if (notify)
        {
            drawingNode.NotifySelectionChanged();
        }
    }

    public static Rect CalculateSelectedRect(ItemsControl? itemsControl)
    {
        if (itemsControl?.DataContext is not IDrawingNode drawingNode)
        {
            return default;
        }

        var selectedRect = new Rect();
        
        itemsControl.UpdateLayout();

        var selectedNodes = drawingNode.GetSelectedNodes();
        var selectedConnectors = drawingNode.GetSelectedConnectors();

        if (selectedNodes is { Count: > 0 } && drawingNode.Nodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                var index = drawingNode.Nodes.IndexOf(node);
                var selectedControl = itemsControl.ContainerFromIndex(index);
                var bounds = selectedControl?.Bounds ?? default;
                selectedRect = selectedRect == default ? bounds : selectedRect.Union(bounds);
            }
        }

        if (selectedConnectors is { Count: > 0 } && drawingNode.Connectors is { Count: > 0 })
        {
            foreach (var connector in selectedConnectors)
            {
                var bounds = GetConnectorBounds(connector);
                selectedRect = selectedRect == default ? bounds : selectedRect.Union(bounds);
            }
        }

        return selectedRect;
    }
}
