using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class NodeRotateBehavior : Behavior<Thumb>
{
    public static readonly StyledProperty<INode?> NodeSourceProperty =
        AvaloniaProperty.Register<NodeRotateBehavior, INode?>(nameof(NodeSource));

    public static readonly StyledProperty<IBrush?> AngleReadoutBackgroundProperty =
        AvaloniaProperty.Register<NodeRotateBehavior, IBrush?>(nameof(AngleReadoutBackground));

    public static readonly StyledProperty<IBrush?> AngleReadoutBorderBrushProperty =
        AvaloniaProperty.Register<NodeRotateBehavior, IBrush?>(nameof(AngleReadoutBorderBrush));

    public static readonly StyledProperty<IBrush?> AngleReadoutForegroundProperty =
        AvaloniaProperty.Register<NodeRotateBehavior, IBrush?>(nameof(AngleReadoutForeground));

    private const double RotationSnapStep = 15.0;
    private static readonly IBrush DefaultReadoutBackground =
        new ImmutableSolidColorBrush(Color.FromArgb(0xCC, 0x1E, 0x1E, 0x1E));
    private static readonly IBrush DefaultReadoutBorder =
        new ImmutableSolidColorBrush(Color.FromArgb(0xFF, 0x17, 0x9D, 0xE3));
    private static readonly IBrush DefaultReadoutForeground =
        new ImmutableSolidColorBrush(Colors.White);
    private bool _isDragging;
    private bool _undoActive;
    private double _startRotation;
    private double _startAngle;
    private Point _center;
    private Visual? _referenceVisual;
    private Canvas? _adornerCanvas;
    private GuidesAdorner? _guidesAdorner;
    private Border? _angleReadout;
    private TextBlock? _angleReadoutText;

    public INode? NodeSource
    {
        get => GetValue(NodeSourceProperty);
        set => SetValue(NodeSourceProperty, value);
    }

    public IBrush? AngleReadoutBackground
    {
        get => GetValue(AngleReadoutBackgroundProperty);
        set => SetValue(AngleReadoutBackgroundProperty, value);
    }

    public IBrush? AngleReadoutBorderBrush
    {
        get => GetValue(AngleReadoutBorderBrushProperty);
        set => SetValue(AngleReadoutBorderBrushProperty, value);
    }

    public IBrush? AngleReadoutForeground
    {
        get => GetValue(AngleReadoutForegroundProperty);
        set => SetValue(AngleReadoutForegroundProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AngleReadoutBackgroundProperty
            || change.Property == AngleReadoutBorderBrushProperty
            || change.Property == AngleReadoutForegroundProperty)
        {
            ApplyAngleReadoutTheme();
        }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
        AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
        RemoveGuides();
        RemoveAngleReadout();
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject is null || NodeSource is not INode node || !node.CanResize())
        {
            return;
        }

        if (node.Parent is not IDrawingNode drawingNode)
        {
            return;
        }

        var info = e.GetCurrentPoint(AssociatedObject);
        if (!info.Properties.IsLeftButtonPressed)
        {
            return;
        }

        var reference = ResolveReferenceVisual();
        if (reference is null)
        {
            return;
        }

        var position = e.GetPosition(reference);
        _center = new Point(node.X + node.Width / 2.0, node.Y + node.Height / 2.0);
        _startAngle = GetAngle(_center, position);
        _startRotation = node.Rotation;
        _isDragging = true;
        BeginUndo(drawingNode);
        e.Pointer.Capture(AssociatedObject);
        e.Handled = true;
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || NodeSource is not INode node || node.Parent is not IDrawingNode drawingNode)
        {
            return;
        }

        var reference = ResolveReferenceVisual();
        if (reference is null)
        {
            return;
        }

        var position = e.GetPosition(reference);
        var angle = GetAngle(_center, position);
        var delta = DeltaAngle(_startAngle, angle);
        var rotation = _startRotation + delta;
        var snapped = ApplyRotationSnap(drawingNode, node, rotation, out var guides, out var snapAngle);

        node.Rotation = snapped;

        if (guides.Count > 0)
        {
            UpdateGuides(guides);
        }
        else
        {
            RemoveGuides();
        }

        if (snapAngle.HasValue)
        {
            UpdateAngleReadout(snapAngle.Value);
        }
        else
        {
            RemoveAngleReadout();
        }

        var selectedNodes = drawingNode.GetSelectedNodes();
        if (selectedNodes is not null && selectedNodes.Contains(node))
        {
            drawingNode.NotifySelectionChanged();
        }

        e.Handled = true;
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        EndUndo();
        RemoveGuides();
        RemoveAngleReadout();
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        EndUndo();
        RemoveGuides();
        RemoveAngleReadout();
    }

    private double ApplyRotationSnap(IDrawingNode drawingNode, INode node, double rotation, out List<GuideLine> guides, out double? snapAngle)
    {
        guides = new List<GuideLine>();
        snapAngle = null;

        var settings = drawingNode.Settings;
        var enableSnap = settings.EnableSnap || settings.EnableGuides;
        var showGuides = settings.EnableGuides;
        if (!enableSnap || settings.GuideSnapTolerance <= 0.0)
        {
            return rotation;
        }

        var tolerance = settings.GuideSnapTolerance;
        var target = NormalizeAngle(rotation);
        var bestDiff = double.PositiveInfinity;
        double? bestCandidate = null;

        for (var angle = 0.0; angle < 360.0; angle += RotationSnapStep)
        {
            var diff = AngleDifference(target, angle);
            if (diff <= tolerance && diff < bestDiff)
            {
                bestDiff = diff;
                bestCandidate = angle;
            }
        }

        if (drawingNode.Nodes is { Count: > 0 })
        {
            foreach (var other in drawingNode.Nodes)
            {
                if (ReferenceEquals(other, node))
                {
                    continue;
                }

                if (!other.CanSelect())
                {
                    continue;
                }

                var candidate = NormalizeAngle(other.Rotation);
                var diff = AngleDifference(target, candidate);
                if (diff <= tolerance && diff < bestDiff)
                {
                    bestDiff = diff;
                    bestCandidate = candidate;
                }
            }
        }

        if (bestCandidate.HasValue)
        {
            var snapped = bestCandidate.Value;
            snapAngle = snapped;
            if (showGuides)
            {
                var length = Math.Max(drawingNode.Width, drawingNode.Height);
                if (length <= 0.0)
                {
                    length = 1000.0;
                }

                var radians = snapped * Math.PI / 180.0;
                var dx = Math.Cos(radians) * length;
                var dy = Math.Sin(radians) * length;
                var start = new Point(_center.X - dx, _center.Y - dy);
                var end = new Point(_center.X + dx, _center.Y + dy);
                guides.Add(new GuideLine(start, end));
            }
            return snapped;
        }

        return rotation;
    }

    private static double GetAngle(Point center, Point point)
    {
        return Math.Atan2(point.Y - center.Y, point.X - center.X) * 180.0 / Math.PI;
    }

    private static double DeltaAngle(double from, double to)
    {
        var diff = to - from;
        while (diff > 180.0)
        {
            diff -= 360.0;
        }

        while (diff < -180.0)
        {
            diff += 360.0;
        }

        return diff;
    }

    private static double NormalizeAngle(double angle)
    {
        angle %= 360.0;
        if (angle < 0.0)
        {
            angle += 360.0;
        }

        return angle;
    }

    private static double AngleDifference(double a, double b)
    {
        var diff = Math.Abs(a - b) % 360.0;
        return diff > 180.0 ? 360.0 - diff : diff;
    }

    private Visual? ResolveReferenceVisual()
    {
        if (_referenceVisual is not null)
        {
            return _referenceVisual;
        }

        if (AssociatedObject is not Visual visual)
        {
            return null;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is ItemsControl)
            {
                _referenceVisual = ancestor;
                break;
            }
        }

        return _referenceVisual;
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

    private void UpdateAngleReadout(double angle)
    {
        var layer = ResolveAdornerCanvas();
        if (layer is null)
        {
            return;
        }

        if (_angleReadout is null)
        {
            _angleReadoutText = new TextBlock
            {
                FontSize = 12
            };

            _angleReadout = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                Child = _angleReadoutText,
                IsHitTestVisible = false
            };

            _angleReadout.SetValue(Panel.ZIndexProperty, 10);
            layer.Children.Add(_angleReadout);
        }

        ApplyAngleReadoutTheme();

        var normalized = NormalizeAngle(angle);
        _angleReadoutText!.Text = $"{Math.Round(normalized)} deg";

        var position = new Point(_center.X + 12.0, _center.Y - 28.0);
        Canvas.SetLeft(_angleReadout, position.X);
        Canvas.SetTop(_angleReadout, position.Y);
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

    private void RemoveAngleReadout()
    {
        var layer = ResolveAdornerCanvas();
        if (layer is null || _angleReadout is null)
        {
            return;
        }

        layer.Children.Remove(_angleReadout);
        _angleReadout = null;
        _angleReadoutText = null;
    }

    private void ApplyAngleReadoutTheme()
    {
        if (_angleReadout is not null)
        {
            _angleReadout.Background = AngleReadoutBackground ?? DefaultReadoutBackground;
            _angleReadout.BorderBrush = AngleReadoutBorderBrush ?? DefaultReadoutBorder;
        }

        if (_angleReadoutText is not null)
        {
            _angleReadoutText.Foreground = AngleReadoutForeground ?? DefaultReadoutForeground;
        }
    }

    private void BeginUndo(IDrawingNode drawingNode)
    {
        if (_undoActive)
        {
            return;
        }

        if (drawingNode is IUndoRedoHost host)
        {
            host.BeginUndoBatch();
            _undoActive = true;
        }
    }

    private void EndUndo()
    {
        if (!_undoActive)
        {
            return;
        }

        if (NodeSource?.Parent is IUndoRedoHost host)
        {
            host.EndUndoBatch();
        }

        _undoActive = false;
    }
}
