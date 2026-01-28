using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class NodeResizeBehavior : Behavior<Thumb>
{
    public static readonly StyledProperty<INode?> NodeSourceProperty =
        AvaloniaProperty.Register<NodeResizeBehavior, INode?>(nameof(NodeSource));

    public static readonly StyledProperty<NodeResizeDirection> DirectionProperty =
        AvaloniaProperty.Register<NodeResizeBehavior, NodeResizeDirection>(nameof(Direction));

    private double _startX;
    private double _startY;
    private double _startWidth;
    private double _startHeight;
    private double _pendingX;
    private double _pendingY;
    private bool _isDragging;
    private bool _undoActive;
    private GuidesAdorner? _guidesAdorner;
    private Canvas? _adornerCanvas;

    public INode? NodeSource
    {
        get => GetValue(NodeSourceProperty);
        set => SetValue(NodeSourceProperty, value);
    }

    public NodeResizeDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.DragStarted += DragStarted;
        AssociatedObject.DragDelta += DragDelta;
        AssociatedObject.DragCompleted += DragCompleted;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.DragStarted -= DragStarted;
        AssociatedObject.DragDelta -= DragDelta;
        AssociatedObject.DragCompleted -= DragCompleted;
        RemoveGuides();
    }

    private void DragStarted(object? sender, VectorEventArgs e)
    {
        if (NodeSource is not INode node || !node.CanResize())
        {
            return;
        }

        _startX = node.X;
        _startY = node.Y;
        _startWidth = node.Width;
        _startHeight = node.Height;
        _pendingX = 0.0;
        _pendingY = 0.0;
        _isDragging = true;
        BeginUndo(node);
        e.Handled = true;
    }

    private void DragDelta(object? sender, VectorEventArgs e)
    {
        if (!_isDragging || NodeSource is not INode node || !node.CanResize())
        {
            return;
        }

        _pendingX += e.Vector.X;
        _pendingY += e.Vector.Y;

        var targetX = _startX;
        var targetY = _startY;
        var targetWidth = _startWidth;
        var targetHeight = _startHeight;

        switch (Direction)
        {
            case NodeResizeDirection.Left:
                targetX = _startX + _pendingX;
                targetWidth = _startWidth - _pendingX;
                break;
            case NodeResizeDirection.Right:
                targetWidth = _startWidth + _pendingX;
                break;
            case NodeResizeDirection.Top:
                targetY = _startY + _pendingY;
                targetHeight = _startHeight - _pendingY;
                break;
            case NodeResizeDirection.Bottom:
                targetHeight = _startHeight + _pendingY;
                break;
            case NodeResizeDirection.TopLeft:
                targetX = _startX + _pendingX;
                targetWidth = _startWidth - _pendingX;
                targetY = _startY + _pendingY;
                targetHeight = _startHeight - _pendingY;
                break;
            case NodeResizeDirection.TopRight:
                targetWidth = _startWidth + _pendingX;
                targetY = _startY + _pendingY;
                targetHeight = _startHeight - _pendingY;
                break;
            case NodeResizeDirection.BottomLeft:
                targetX = _startX + _pendingX;
                targetWidth = _startWidth - _pendingX;
                targetHeight = _startHeight + _pendingY;
                break;
            case NodeResizeDirection.BottomRight:
                targetWidth = _startWidth + _pendingX;
                targetHeight = _startHeight + _pendingY;
                break;
        }

        ApplySnap(node, ref targetX, ref targetY, ref targetWidth, ref targetHeight);
        ApplyGuideSnap(node, ref targetX, ref targetY, ref targetWidth, ref targetHeight);

        var deltaX = 0.0;
        var deltaY = 0.0;

        if (Direction is NodeResizeDirection.Left or NodeResizeDirection.TopLeft or NodeResizeDirection.BottomLeft)
        {
            deltaX = targetX - node.X;
        }
        else if (Direction is NodeResizeDirection.Right or NodeResizeDirection.TopRight or NodeResizeDirection.BottomRight)
        {
            deltaX = targetWidth - node.Width;
        }

        if (Direction is NodeResizeDirection.Top or NodeResizeDirection.TopLeft or NodeResizeDirection.TopRight)
        {
            deltaY = targetY - node.Y;
        }
        else if (Direction is NodeResizeDirection.Bottom or NodeResizeDirection.BottomLeft or NodeResizeDirection.BottomRight)
        {
            deltaY = targetHeight - node.Height;
        }

        node.Resize(deltaX, deltaY, Direction);
        node.OnResized();

        if (node.Parent is IDrawingNode drawingNode)
        {
            var selectedNodes = drawingNode.GetSelectedNodes();
            if (selectedNodes is not null && selectedNodes.Contains(node))
            {
                drawingNode.NotifySelectionChanged();
            }
        }
        e.Handled = true;
    }

    private void DragCompleted(object? sender, VectorEventArgs e)
    {
        if (_isDragging && NodeSource is INode node)
        {
            EndUndo(node);
        }

        RemoveGuides();
        _isDragging = false;
        _pendingX = 0.0;
        _pendingY = 0.0;
        e.Handled = true;
    }

    private void ApplySnap(INode node, ref double x, ref double y, ref double width, ref double height)
    {
        if (node.Parent is not IDrawingNode drawingNode)
        {
            return;
        }

        var settings = drawingNode.Settings;
        if (!settings.EnableSnap)
        {
            return;
        }

        var snapX = settings.SnapX;
        var snapY = settings.SnapY;

        var left = x;
        var right = x + width;
        var top = y;
        var bottom = y + height;

        if (snapX > 0.0)
        {
            if (Direction is NodeResizeDirection.Left or NodeResizeDirection.TopLeft or NodeResizeDirection.BottomLeft)
            {
                left = SnapHelper.Snap(left, snapX);
            }
            else if (Direction is NodeResizeDirection.Right or NodeResizeDirection.TopRight or NodeResizeDirection.BottomRight)
            {
                right = SnapHelper.Snap(right, snapX);
            }
        }

        if (snapY > 0.0)
        {
            if (Direction is NodeResizeDirection.Top or NodeResizeDirection.TopLeft or NodeResizeDirection.TopRight)
            {
                top = SnapHelper.Snap(top, snapY);
            }
            else if (Direction is NodeResizeDirection.Bottom or NodeResizeDirection.BottomLeft or NodeResizeDirection.BottomRight)
            {
                bottom = SnapHelper.Snap(bottom, snapY);
            }
        }

        x = left;
        y = top;
        width = right - left;
        height = bottom - top;
    }

    private void ApplyGuideSnap(INode node, ref double x, ref double y, ref double width, ref double height)
    {
        if (node.Parent is not IDrawingNode drawingNode)
        {
            RemoveGuides();
            return;
        }

        var settings = drawingNode.Settings;
        if (!settings.EnableGuides || settings.GuideSnapTolerance <= 0.0)
        {
            RemoveGuides();
            return;
        }

        if (drawingNode.Nodes is not { Count: > 0 } nodes)
        {
            RemoveGuides();
            return;
        }

        var moveLeft = Direction is NodeResizeDirection.Left or NodeResizeDirection.TopLeft or NodeResizeDirection.BottomLeft;
        var moveRight = Direction is NodeResizeDirection.Right or NodeResizeDirection.TopRight or NodeResizeDirection.BottomRight;
        var moveTop = Direction is NodeResizeDirection.Top or NodeResizeDirection.TopLeft or NodeResizeDirection.TopRight;
        var moveBottom = Direction is NodeResizeDirection.Bottom or NodeResizeDirection.BottomLeft or NodeResizeDirection.BottomRight;

        var bounds = new Rect(x, y, width, height);
        var left = bounds.Left;
        var right = bounds.Right;
        var top = bounds.Top;
        var bottom = bounds.Bottom;

        var tolerance = settings.GuideSnapTolerance;
        var bestXDiff = double.PositiveInfinity;
        var bestYDiff = double.PositiveInfinity;
        GuideLine? bestXGuide = null;
        GuideLine? bestYGuide = null;
        var guides = new List<GuideLine>();

        foreach (var other in nodes)
        {
            if (ReferenceEquals(other, node))
            {
                continue;
            }

            if (!other.CanSelect())
            {
                continue;
            }

            var otherBounds = new Rect(other.X, other.Y, other.Width, other.Height);

            if (moveLeft || moveRight)
            {
                var movingX = moveLeft ? left : right;
                var candidatesX = new[]
                {
                    otherBounds.Left,
                    otherBounds.Center.X,
                    otherBounds.Right
                };

                foreach (var candidateX in candidatesX)
                {
                    var diff = candidateX - movingX;
                    var abs = Math.Abs(diff);
                    if (abs > tolerance || abs >= bestXDiff)
                    {
                        continue;
                    }

                    bestXDiff = abs;
                    var lineTop = Math.Min(top, otherBounds.Top);
                    var lineBottom = Math.Max(bottom, otherBounds.Bottom);
                    bestXGuide = new GuideLine(new Point(candidateX, lineTop), new Point(candidateX, lineBottom));
                }
            }

            if (moveTop || moveBottom)
            {
                var movingY = moveTop ? top : bottom;
                var candidatesY = new[]
                {
                    otherBounds.Top,
                    otherBounds.Center.Y,
                    otherBounds.Bottom
                };

                foreach (var candidateY in candidatesY)
                {
                    var diff = candidateY - movingY;
                    var abs = Math.Abs(diff);
                    if (abs > tolerance || abs >= bestYDiff)
                    {
                        continue;
                    }

                    bestYDiff = abs;
                    var lineLeft = Math.Min(left, otherBounds.Left);
                    var lineRight = Math.Max(right, otherBounds.Right);
                    bestYGuide = new GuideLine(new Point(lineLeft, candidateY), new Point(lineRight, candidateY));
                }
            }
        }

        if (bestXGuide.HasValue)
        {
            var candidateX = bestXGuide.Value.Start.X;
            if (moveLeft)
            {
                left += candidateX - left;
            }
            else if (moveRight)
            {
                right += candidateX - right;
            }

            guides.Add(bestXGuide.Value);
        }

        if (bestYGuide.HasValue)
        {
            var candidateY = bestYGuide.Value.Start.Y;
            if (moveTop)
            {
                top += candidateY - top;
            }
            else if (moveBottom)
            {
                bottom += candidateY - bottom;
            }

            guides.Add(bestYGuide.Value);
        }

        if (guides.Count > 0)
        {
            x = left;
            y = top;
            width = right - left;
            height = bottom - top;
            UpdateGuides(guides);
        }
        else
        {
            RemoveGuides();
        }
    }

    private Canvas? ResolveAdornerCanvas()
    {
        if (_adornerCanvas is not null)
        {
            return _adornerCanvas;
        }

        if (AssociatedObject is not Visual visual)
        {
            return null;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is DrawingNode drawingNode)
            {
                _adornerCanvas = drawingNode.AdornerCanvas;
                break;
            }
        }

        return _adornerCanvas;
    }

    private void UpdateGuides(IReadOnlyList<GuideLine> guides)
    {
        var layer = ResolveAdornerCanvas();
        if (layer is null)
        {
            return;
        }

        if (_guidesAdorner is null)
        {
            _guidesAdorner = new GuidesAdorner
            {
                IsHitTestVisible = false
            };
            layer.Children.Add(_guidesAdorner);
        }

        _guidesAdorner.Guides = guides;
        layer.InvalidateVisual();
    }

    private void RemoveGuides()
    {
        var layer = ResolveAdornerCanvas();
        if (layer is null || _guidesAdorner is null)
        {
            return;
        }

        layer.Children.Remove(_guidesAdorner);
        _guidesAdorner = null;
    }

    private void BeginUndo(INode node)
    {
        if (_undoActive)
        {
            return;
        }

        if (node.Parent is IUndoRedoHost host)
        {
            host.BeginUndoBatch();
            _undoActive = true;
        }
    }

    private void EndUndo(INode node)
    {
        if (!_undoActive)
        {
            return;
        }

        if (node.Parent is IUndoRedoHost host)
        {
            host.EndUndoBatch();
        }

        _undoActive = false;
    }
}
