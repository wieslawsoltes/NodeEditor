using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors
{
    public class DrawingSelectionBehavior : Behavior<ItemsControl>
    {
        private IDisposable? _dataContextDisposable;
        private INotifyPropertyChanged? _drawingNodePropertyChanged;
        private IDrawingNode? _drawingNode;
        private Selection? _selection;
        private Selected? _selected;
        private bool _dragSelectedItems;
        private Point _start;
        private Rect _selectedRect;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is { })
            {
                AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);

                _dataContextDisposable = AssociatedObject
                    .GetObservable(StyledElement.DataContextProperty)
                    .Subscribe(x =>
                    {
                        if (x is IDrawingNode drawingNode)
                        {
                            if (_drawingNode == drawingNode)
                            {
                                if (_drawingNodePropertyChanged != null)
                                {
                                    _drawingNodePropertyChanged.PropertyChanged -= DrawingNode_PropertyChanged;
                                }
                            }

                            RemoveSelection(AssociatedObject);
                            RemoveSelected(AssociatedObject);

                            _drawingNode = drawingNode;

                            if (_drawingNode is INotifyPropertyChanged notifyPropertyChanged)
                            {
                                _drawingNodePropertyChanged = notifyPropertyChanged;
                                _drawingNodePropertyChanged.PropertyChanged += DrawingNode_PropertyChanged;
                            }
                        }
                        else
                        {
                            RemoveSelection(AssociatedObject);
                            RemoveSelected(AssociatedObject);
                        }
                    });
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is { })
            {
                AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
                AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
                AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);

                if (_drawingNodePropertyChanged is { })
                {
                    _drawingNodePropertyChanged.PropertyChanged -= DrawingNode_PropertyChanged;
                }

                _dataContextDisposable?.Dispose();
            }
        }

        private void DrawingNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode)
            {
                return;
            }

            if (e.PropertyName == nameof(IDrawingNode.SelectedNodes) || e.PropertyName == nameof(IDrawingNode.SelectedConnectors))
            {
                if (_drawingNode is { })
                {
                    if (_drawingNode.SelectedNodes is { Count: > 0 } || _drawingNode.SelectedConnectors is { Count: > 0 })
                    {
                        _selectedRect = CalculateSelectedRect();

                        if (_selected is { })
                        {
                            RemoveSelected(AssociatedObject);
                        }

                        if (!_selectedRect.IsEmpty && _selected is null)
                        {
                            AddSelected(AssociatedObject, _selectedRect);
                        }
                    }
                    else
                    {
                        RemoveSelected(AssociatedObject);
                    }
                }
            }
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return;
            }

            var position = e.GetPosition(AssociatedObject);

            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                _dragSelectedItems = false;

                if (drawingNode.SelectedNodes is { Count: > 0 } || drawingNode.SelectedConnectors is { Count: > 0 })
                {
                    if (_selectedRect.Contains(position))
                    {
                        _dragSelectedItems = true;
                        _start = position;
                        e.Handled = true;
                    }
                    else
                    {
                        drawingNode.SelectedNodes = null;
                        drawingNode.SelectedConnectors = null;
                        RemoveSelected(AssociatedObject);
                    }
                }

                if (!_dragSelectedItems)
                {
                    drawingNode.SelectedNodes = null;
                    drawingNode.SelectedConnectors = null;
                    RemoveSelected(AssociatedObject);

                    if (e.Source is Control { DataContext: not IDrawingNode })
                    {
                        return;
                    }

                    AddSelection(AssociatedObject, position.X, position.Y);
                }
            }
        }

        private void Released(object? sender, PointerReleasedEventArgs e)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode)
            {
                return;
            }

            _dragSelectedItems = false;

            if (e.Source is Control { DataContext: not IDrawingNode })
            {
                return;
            }

            if (_selection is { })
            {
                FindSelectedNodes(_selection.GetRect());
            }

            RemoveSelection(AssociatedObject);
        }

        private static double Length(Point pt0, Point pt1)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt0.X, 2) + Math.Pow(pt1.Y - pt0.Y, 2));
        }
        
        private static Point[] FlattenCubic(Point pt0, Point pt1, Point pt2, Point pt3)
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

        private static bool HitTestConnector(IConnector connector, Rect rect)
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

        private static Rect GetConnectorBounds(IConnector connector)
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

        private void FindSelectedNodes(Rect rect)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return;
            }

            drawingNode.SelectedNodes = null;
            drawingNode.SelectedConnectors = null;

            var selectedNodes = new HashSet<INode>();

            foreach (var container in AssociatedObject.ItemContainerGenerator.Containers)
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

        private void ExecuteLayoutPass()
        {
            if (AssociatedObject?.GetVisualRoot() is TopLevel topLevel)
            {
                topLevel.LayoutManager.ExecuteLayoutPass();
            }
        }

        private Rect CalculateSelectedRect()
        {
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return Rect.Empty;
            }

            var selectedRect = new Rect();

            ExecuteLayoutPass();

            if (drawingNode.SelectedNodes is { Count: > 0 } && drawingNode.Nodes is { Count: > 0 })
            {
                foreach (var node in drawingNode.SelectedNodes)
                {
                    var index = drawingNode.Nodes.IndexOf(node);
                    var selectedControl = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(index);
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

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode)
            {
                return;
            }

            var position = e.GetPosition(AssociatedObject);

            if (_dragSelectedItems)
            {
                Move(position);

                e.Handled = true;
            }
            else
            {
                if (e.Source is Control { DataContext: not IDrawingNode })
                {
                    return;
                }

                UpdateSelection(position.X, position.Y);
            }
        }

        private void Move(Point position)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return;
            }

            if (drawingNode.SelectedNodes is not { Count: > 0 } || drawingNode.Nodes is not { Count: > 0 })
            {
                return;
            }

            var deltaX = position.X - _start.X;
            var deltaY = position.Y - _start.Y;
            _start = position;

            var selectedRect = CalculateSelectedRect();

            _selectedRect = selectedRect;

            foreach (var node in drawingNode.SelectedNodes)
            {
                node.X += deltaX;
                node.Y += deltaY;
            }

            UpdateSelected(selectedRect);
        }

        private void AddSelection(Control control, double x, double y)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null)
            {
                return;
            }

            _selection = new Selection
            {
                [AdornerLayer.AdornedElementProperty] = control,
                TopLeft = new Point(x, y),
                BottomRight = new Point(x, y)
            };

            ((ISetLogicalParent) _selection).SetParent(control);
            layer.Children.Add(_selection);
        }

        private void RemoveSelection(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null || _selection is null)
            {
                return;
            }

            layer.Children.Remove(_selection);
            ((ISetLogicalParent) _selection).SetParent(null);
            _selection = null;
        }

        private void UpdateSelection(double x, double y)
        {
            if (_selection is { } selection)
            {
                selection.BottomRight = new Point(x, y);
            }
        }

        private void AddSelected(Control control, Rect rect)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null)
            {
                return;
            }

            _selected = new Selected
            {
                [AdornerLayer.AdornedElementProperty] = control,
                IsHitTestVisible = false,
                Rect = rect
            };

            ((ISetLogicalParent) _selected).SetParent(control);
            layer.Children.Add(_selected);
        }

        private void RemoveSelected(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null || _selected is null)
            {
                return;
            }

            layer.Children.Remove(_selected);
            ((ISetLogicalParent) _selected).SetParent(null);
            _selected = null;
        }

        private void UpdateSelected(Rect rect)
        {
            if (_selected is { } selected)
            {
                selected.Rect = rect;
            }
        }
    }
}
