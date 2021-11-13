using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using NodeEditor.Model;

namespace NodeEditor
{
    internal static class HitTest
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
                return Rect.Empty;
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

            drawingNode.SelectedNodes = null;
            drawingNode.SelectedConnectors = null;

            var selectedNodes = new HashSet<INode>();

            foreach (var container in itemsControl.ItemContainerGenerator.Containers)
            {
                if (container.ContainerControl is not { DataContext: INode node } containerControl)
                {
                    continue;
                }

                var bounds = containerControl.Bounds;

                if (!rect.Intersects(bounds))
                {
                    continue;
                }

                selectedNodes.Add(node);
            }

            if (drawingNode.Connectors is { Count: > 0 })
            {
                var selectedConnectors = new HashSet<IConnector>();

                foreach (var connector in drawingNode.Connectors)
                {
                    if (HitTestConnector(connector, rect))
                    {
                        selectedConnectors.Add(connector);
                    }
                }

                if (selectedConnectors.Count > 0)
                {
                    drawingNode.SelectedConnectors = selectedConnectors;
                }
            }

            if (selectedNodes.Count > 0)
            {
                drawingNode.SelectedNodes = selectedNodes;
            }
        }

        public static Rect CalculateSelectedRect(ItemsControl? itemsControl)
        {
            if (itemsControl?.DataContext is not IDrawingNode drawingNode)
            {
                return Rect.Empty;
            }

            var selectedRect = new Rect();

            if (itemsControl.GetVisualRoot() is TopLevel topLevel)
            {
                topLevel.LayoutManager.ExecuteLayoutPass();
            }

            if (drawingNode.SelectedNodes is { Count: > 0 } && drawingNode.Nodes is { Count: > 0 })
            {
                foreach (var node in drawingNode.SelectedNodes)
                {
                    var index = drawingNode.Nodes.IndexOf(node);
                    var selectedControl = itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
                    var bounds = selectedControl.Bounds;
                    selectedRect = selectedRect.IsEmpty ? bounds : selectedRect.Union(bounds);
                }
            }

            if (drawingNode.SelectedConnectors is { Count: > 0 } && drawingNode.Connectors is { Count: > 0 })
            {
                foreach (var connector in drawingNode.SelectedConnectors)
                {
                    var bounds = GetConnectorBounds(connector);
                    selectedRect = selectedRect.IsEmpty ? bounds : selectedRect.Union(bounds);
                }
            }

            return selectedRect;
        }
    }
}
