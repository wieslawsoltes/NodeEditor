using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class InkLayer : Control
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<InkLayer, IDrawingNode?>(nameof(DrawingSource));

    private InkStroke? _activeStroke;
    private INotifyCollectionChanged? _strokesNotifier;
    private INotifyPropertyChanged? _drawingNotifier;
    private bool _undoActive;

    public InkLayer()
    {
        ClipToBounds = true;
    }

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DrawingSourceProperty)
        {
            DetachDrawing(change.OldValue as IDrawingNode);
            AttachDrawing(change.NewValue as IDrawingNode);
            InvalidateVisual();
        }
    }

    private void AttachDrawing(IDrawingNode? drawing)
    {
        if (drawing is null)
        {
            return;
        }

        if (drawing is INotifyPropertyChanged notify)
        {
            _drawingNotifier = notify;
            _drawingNotifier.PropertyChanged += OnDrawingPropertyChanged;
        }

        AttachStrokes(drawing.InkStrokes);
    }

    private void DetachDrawing(IDrawingNode? drawing)
    {
        if (_drawingNotifier is not null)
        {
            _drawingNotifier.PropertyChanged -= OnDrawingPropertyChanged;
            _drawingNotifier = null;
        }

        DetachStrokes();
    }

    private void OnDrawingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDrawingNode.InkStrokes))
        {
            DetachStrokes();
            AttachStrokes(DrawingSource?.InkStrokes);
            InvalidateVisual();
        }
    }

    private void AttachStrokes(IList<InkStroke>? strokes)
    {
        if (strokes is INotifyCollectionChanged notify)
        {
            _strokesNotifier = notify;
            _strokesNotifier.CollectionChanged += OnStrokesChanged;
        }
    }

    private void DetachStrokes()
    {
        if (_strokesNotifier is not null)
        {
            _strokesNotifier.CollectionChanged -= OnStrokesChanged;
            _strokesNotifier = null;
        }
    }

    private void OnStrokesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (DrawingSource is not IDrawingNode drawing || !IsInkActive(drawing))
        {
            return;
        }

        var info = e.GetCurrentPoint(this);
        if (!info.Properties.IsLeftButtonPressed)
        {
            return;
        }

        var point = e.GetPosition(this);
        var stroke = CreateStroke(drawing);
        stroke.Points.Add(new InkPoint(point.X, point.Y, 1.0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

        if (drawing is IUndoRedoHost host)
        {
            host.BeginUndoBatch();
            _undoActive = true;
        }

        drawing.InkStrokes ??= new System.Collections.ObjectModel.ObservableCollection<InkStroke>();
        drawing.InkStrokes.Add(stroke);

        _activeStroke = stroke;
        e.Pointer.Capture(this);
        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeStroke is null || DrawingSource is not IDrawingNode drawing || !IsInkActive(drawing))
        {
            return;
        }

        var position = e.GetPosition(this);
        if (ShouldAppendPoint(_activeStroke, position))
        {
            _activeStroke.Points.Add(new InkPoint(position.X, position.Y, 1.0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            InvalidateVisual();
        }

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_activeStroke is null)
        {
            return;
        }

        e.Pointer.Capture(null);
        _activeStroke = null;
        if (_undoActive && DrawingSource is IUndoRedoHost host)
        {
            host.EndUndoBatch();
        }

        _undoActive = false;
        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        if (_activeStroke is null)
        {
            return;
        }

        _activeStroke = null;
        if (_undoActive && DrawingSource is IUndoRedoHost host)
        {
            host.EndUndoBatch();
        }

        _undoActive = false;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (DrawingSource?.InkStrokes is not { Count: > 0 } strokes)
        {
            return;
        }

        foreach (var stroke in strokes)
        {
            RenderStroke(context, stroke);
        }
    }

    private static bool IsInkActive(IDrawingNode drawing)
    {
        return drawing.Settings.EnableInk && drawing.Settings.IsInkMode;
    }

    private static bool ShouldAppendPoint(InkStroke stroke, Point position)
    {
        if (stroke.Points.Count == 0)
        {
            return true;
        }

        var last = stroke.Points[stroke.Points.Count - 1];
        var dx = last.X - position.X;
        var dy = last.Y - position.Y;
        return (dx * dx + dy * dy) >= 1.0;
    }

    private static InkStroke CreateStroke(IDrawingNode drawing)
    {
        var pen = drawing.Settings.ActivePen;
        if (pen is null)
        {
            return new InkStroke();
        }

        return new InkStroke
        {
            Color = pen.Color,
            Thickness = pen.Thickness,
            Opacity = pen.Opacity,
            Name = pen.Name
        };
    }

    internal static void RenderStroke(DrawingContext context, InkStroke stroke)
    {
        if (stroke.Points is null || stroke.Points.Count == 0)
        {
            return;
        }

        var color = InkColorHelper.ToColor(stroke.Color, stroke.Opacity);
        var brush = new ImmutableSolidColorBrush(color);
        var thickness = Math.Max(0.5, stroke.Thickness);
        var pen = new ImmutablePen(brush, thickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

        if (stroke.Points.Count == 1)
        {
            var single = stroke.Points[0];
            var radius = thickness * 0.5;
            context.DrawEllipse(brush, null, new Point(single.X, single.Y), radius, radius);
            return;
        }

        var geometry = InkGeometryBuilder.Build(stroke.Points);
        if (geometry is null)
        {
            return;
        }

        context.DrawGeometry(null, pen, geometry);
    }
}
