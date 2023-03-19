using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using System;
using NodeEditor.Mvvm;
using NodeEditorDemo.ViewModels;

namespace NodeEditor.Controls;

public class SelectedAdorner : TemplatedControl, IStyleable

{

    Type IStyleable.StyleKey => typeof(SelectedAdorner);
    public static readonly StyledProperty<bool> EnableResizingProperty =
        AvaloniaProperty.Register<SelectedAdorner, bool>(nameof(EnableResizing));

    public static readonly StyledProperty<bool> EnableDraggingProperty =
        AvaloniaProperty.Register<SelectedAdorner, bool>(nameof(EnableDragging));

    public static readonly StyledProperty<dynamic?> ControlProperty =
        AvaloniaProperty.Register<SelectedAdorner, dynamic?>(nameof(Control));

    public static readonly StyledProperty<Rect> RectProperty =
        AvaloniaProperty.Register<SelectedAdorner, Rect>(nameof(Rect));
    public bool EnableResizing
    {
        get => GetValue(EnableResizingProperty);
        set => SetValue(EnableResizingProperty, value);
    }

    public bool EnableDragging
    {
        get => GetValue(EnableDraggingProperty);
        set => SetValue(EnableDraggingProperty, value);
    }

    public Rect Rect
    {
        get => GetValue(RectProperty);
        set => SetValue(RectProperty, value);
    }

    public dynamic? Control
    {
        get => GetValue(ControlProperty);
        set => SetValue(ControlProperty, value);
    }
    private Canvas? _canvas;
    private Thumb? _drag;
    private Thumb? _top;
    private Thumb? _bottom;
    private Thumb? _left;
    private Thumb? _right;
    private Thumb? _topLeft;
    private Thumb? _topRight;
    private Thumb? _bottomLeft;
    private Thumb? _bottomRight;
    private double _leftOffset;
    private double _topOffset;
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
#pragma warning disable 8631
        base.OnPropertyChanged(change);
#pragma warning restore 8631

        if (change.Property == RectProperty)
        {
            InvalidateMeasure();
            
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var thickness = 2.0;
        var pen = new ImmutablePen(
            new ImmutableSolidColorBrush(new Color(0xFF, 0x17, 0x9D, 0xE3)),
            thickness);
        var bounds = Rect;
        var rect = bounds.Deflate(thickness * 0.5);
        context.DrawRectangle(null, pen, rect);
    }
    public dynamic ControlCrent;
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var visual = GetValue(AdornerLayer.AdornedElementProperty);
        if (visual is not Avalonia.Controls.Control)
        {
            return;
        }


        _canvas = e.NameScope.Find<Canvas>("PART_Canvas");
        _drag = e.NameScope.Find<Thumb>("PART_Drag");
        _top = e.NameScope.Find<Thumb>("PART_Top");
        _bottom = e.NameScope.Find<Thumb>("PART_Bottom");
        _left = e.NameScope.Find<Thumb>("PART_Left");
        _right = e.NameScope.Find<Thumb>("PART_Right");
        _topLeft = e.NameScope.Find<Thumb>("PART_TopLeft");
        _topRight = e.NameScope.Find<Thumb>("PART_TopRight");
        _bottomLeft = e.NameScope.Find<Thumb>("PART_BottomLeft");
        _bottomRight = e.NameScope.Find<Thumb>("PART_BottomRight");

        if (_top is { })
        {
            _top.DragDelta += OnDragDeltaTop;
        }

        if (_bottom is { })
        {
            _bottom.DragDelta += OnDragDeltaBottom;
        }

        if (_left is { })
        {
            _left.DragDelta += OnDragDeltaLeft;
        }

        if (_right is { })
        {
            _right.DragDelta += OnDragDeltaRight;
        }

        if (_topLeft is { })
        {
            _topLeft.DragDelta += OnDragDeltaTopLeft;
        }

        if (_topRight is { })
        {
            _topRight.DragDelta += OnDragDeltaTopRight;
        }

        if (_bottomLeft is { })
        {
            _bottomLeft.DragDelta += OnDragDeltaBottomLeft;
        }

        if (_bottomRight is { })
        {
            _bottomRight.DragDelta += OnDragDeltaBottomRight;
        }

        if (_drag is { })
        {
            _drag.DragDelta += OnDragDeltaDrag;
        }

        if (Control is { })
        {


            foreach (var VARIABLE in Control)
            {
                ControlCrent = (NodeViewModel)VARIABLE;
                break;
            }

            var d = (NodeView)ControlCrent.Content;

            

            // _leftOffset = d.Bounds.Left;
            // _topOffset = c.Bounds.Top;

            var rect = new Rect(
                0,
                0,
                ControlCrent.Width,
                ControlCrent.Height);

            UpdateThumbs(rect);
            UpdateDrag(rect);

            if (_canvas is { })
            {
                Canvas.SetLeft(_canvas, rect.Left);
                Canvas.SetTop(_canvas, rect.Top);
                _canvas.Width = rect.Width;
                _canvas.Height = rect.Height;
            }
        }
    }

    private Rect GetRect()
    {
        var topLeft = new Point(Canvas.GetLeft(_topLeft), Canvas.GetTop(_topLeft));
        var topRight = new Point(Canvas.GetLeft(_topRight), Canvas.GetTop(_topRight));
        var bottomLeft = new Point(Canvas.GetLeft(_bottomLeft), Canvas.GetTop(_bottomLeft));
        var bottomRight = new Point(Canvas.GetLeft(_bottomRight), Canvas.GetTop(_bottomRight));
        var left = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
        var top = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
        var right = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
        var bottom = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));
        var width = Math.Abs(right - left);
        var height = Math.Abs(bottom - top);
        return new Rect(left, top, width, height);
    }

    private void UpdateControl(dynamic control, Rect rect)
    {
        dynamic d = (NodeView)control.Content;

        
        // Canvas.SetLeft(d, rect.Left);
        // Canvas.SetTop(d, rect.Top);

        control.Width = rect.Width;
        control.Height = rect.Height;
    }

    private void UpdateThumbs(Rect rect)
    {
        Canvas.SetLeft(_top, rect.Center.X);
        Canvas.SetTop(_top, rect.Top);

        Canvas.SetLeft(_bottom, rect.Center.X);
        Canvas.SetTop(_bottom, rect.Bottom);

        Canvas.SetLeft(_left, rect.Left);
        Canvas.SetTop(_left, rect.Center.Y);

        Canvas.SetLeft(_right, rect.Right);
        Canvas.SetTop(_right, rect.Center.Y);

        Canvas.SetLeft(_topLeft, rect.Left);
        Canvas.SetTop(_topLeft, rect.Top);

        Canvas.SetLeft(_topRight, rect.Right);
        Canvas.SetTop(_topRight, rect.Top);

        Canvas.SetLeft(_bottomLeft, rect.Left);
        Canvas.SetTop(_bottomLeft, rect.Bottom);

        Canvas.SetLeft(_bottomRight, rect.Right);
        Canvas.SetTop(_bottomRight, rect.Bottom);
    }

    private void UpdateDrag(Rect rect)
    {

        if (_drag is { })
        {
            Canvas.SetLeft(_drag, rect.Left);
            Canvas.SetTop(_drag, rect.Top);

            _drag.Width = rect.Width;
            _drag.Height = rect.Height;
        }
    }

    private void OnDragDeltaDrag(object? sender, VectorEventArgs e)
    {
        Canvas.SetLeft(_top, Canvas.GetLeft(_top) + e.Vector.X);
        Canvas.SetLeft(_bottom, Canvas.GetLeft(_bottom) + e.Vector.X);

        Canvas.SetLeft(_left, Canvas.GetLeft(_left) + e.Vector.X);
        Canvas.SetLeft(_right, Canvas.GetLeft(_right) + e.Vector.X);

        Canvas.SetLeft(_topLeft, Canvas.GetLeft(_topLeft) + e.Vector.X);
        Canvas.SetLeft(_bottomLeft, Canvas.GetLeft(_bottomLeft) + e.Vector.X);

        Canvas.SetLeft(_topRight, Canvas.GetLeft(_topRight) + e.Vector.X);
        Canvas.SetLeft(_bottomRight, Canvas.GetLeft(_bottomRight) + e.Vector.X);

        Canvas.SetTop(_top, Canvas.GetTop(_top) + e.Vector.Y);
        Canvas.SetTop(_bottom, Canvas.GetTop(_bottom) + e.Vector.Y);

        Canvas.SetTop(_left, Canvas.GetTop(_left) + e.Vector.Y);
        Canvas.SetTop(_right, Canvas.GetTop(_right) + e.Vector.Y);

        Canvas.SetTop(_topLeft, Canvas.GetTop(_topLeft) + e.Vector.Y);
        Canvas.SetTop(_topRight, Canvas.GetTop(_topRight) + e.Vector.Y);

        Canvas.SetTop(_bottomLeft, Canvas.GetTop(_bottomLeft) + e.Vector.Y);
        Canvas.SetTop(_bottomRight, Canvas.GetTop(_bottomRight) + e.Vector.Y);

        var rect = GetRect();

        UpdateDrag(rect);

        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }

    private void OnDragDeltaTop(object? sender, VectorEventArgs e)
    {
        Canvas.SetTop(_top, Canvas.GetTop(_top) + e.Vector.Y);
        Canvas.SetTop(_topLeft, Canvas.GetTop(_topLeft) + e.Vector.Y);
        Canvas.SetTop(_topRight, Canvas.GetTop(_topRight) + e.Vector.Y);

        var rect = GetRect();

        Canvas.SetTop(_left, rect.Center.Y);
        Canvas.SetTop(_right, rect.Center.Y);

        UpdateDrag(rect);

        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }

    private void OnDragDeltaBottom(object? sender, VectorEventArgs e)
    {
        Canvas.SetTop(_bottom, Canvas.GetTop(_bottom) + e.Vector.Y);
        Canvas.SetTop(_bottomLeft, Canvas.GetTop(_bottomLeft) + e.Vector.Y);
        Canvas.SetTop(_bottomRight, Canvas.GetTop(_bottomRight) + e.Vector.Y);

        var rect = GetRect();

        Canvas.SetTop(_left, rect.Center.Y);
        Canvas.SetTop(_right, rect.Center.Y);

        UpdateDrag(rect);

        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }

    private void OnDragDeltaLeft(object? sender, VectorEventArgs e)
    {
        Canvas.SetLeft(_left, Canvas.GetLeft(_left) + e.Vector.X);
        Canvas.SetLeft(_topLeft, Canvas.GetLeft(_topLeft) + e.Vector.X);
        Canvas.SetLeft(_bottomLeft, Canvas.GetLeft(_bottomLeft) + e.Vector.X);

        var rect = GetRect();

        Canvas.SetLeft(_top, rect.Center.X);
        Canvas.SetLeft(_bottom, rect.Center.X);

        UpdateDrag(rect);

        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }

    private void OnDragDeltaRight(object? sender, VectorEventArgs e)
    {
        Canvas.SetLeft(_right, Canvas.GetLeft(_right) + e.Vector.X);
        Canvas.SetLeft(_topRight, Canvas.GetLeft(_topRight) + e.Vector.X);
        Canvas.SetLeft(_bottomRight, Canvas.GetLeft(_bottomRight) + e.Vector.X);

        var rect = GetRect();

        Canvas.SetLeft(_top, rect.Center.X);
        Canvas.SetLeft(_bottom, rect.Center.X);

        UpdateDrag(rect);

        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }

    private void OnDragDeltaTopLeft(object? sender, VectorEventArgs e)
    {
        Canvas.SetLeft(_left, Canvas.GetLeft(_left) + e.Vector.X);
        Canvas.SetLeft(_topLeft, Canvas.GetLeft(_topLeft) + e.Vector.X);
        Canvas.SetLeft(_bottomLeft, Canvas.GetLeft(_bottomLeft) + e.Vector.X);

        Canvas.SetTop(_top, Canvas.GetTop(_top) + e.Vector.Y);
        Canvas.SetTop(_topLeft, Canvas.GetTop(_topLeft) + e.Vector.Y);
        Canvas.SetTop(_topRight, Canvas.GetTop(_topRight) + e.Vector.Y);

        var rect = GetRect();

        Canvas.SetLeft(_top, rect.Center.X);
        Canvas.SetLeft(_bottom, rect.Center.X);

        Canvas.SetTop(_left, rect.Center.Y);
        Canvas.SetTop(_right, rect.Center.Y);

        UpdateDrag(rect);


        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }

    private void OnDragDeltaTopRight(object? sender, VectorEventArgs e)
    {
        Canvas.SetLeft(_right, Canvas.GetLeft(_right) + e.Vector.X);
        Canvas.SetLeft(_topRight, Canvas.GetLeft(_topRight) + e.Vector.X);
        Canvas.SetLeft(_bottomRight, Canvas.GetLeft(_bottomRight) + e.Vector.X);

        Canvas.SetTop(_top, Canvas.GetTop(_top) + e.Vector.Y);
        Canvas.SetTop(_topLeft, Canvas.GetTop(_topLeft) + e.Vector.Y);
        Canvas.SetTop(_topRight, Canvas.GetTop(_topRight) + e.Vector.Y);

        var rect = GetRect();

        Canvas.SetLeft(_top, rect.Center.X);
        Canvas.SetLeft(_bottom, rect.Center.X);

        Canvas.SetTop(_left, rect.Center.Y);
        Canvas.SetTop(_right, rect.Center.Y);

        UpdateDrag(rect);


        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }

    private void OnDragDeltaBottomLeft(object? sender, VectorEventArgs e)
    {
        Canvas.SetLeft(_left, Canvas.GetLeft(_left) + e.Vector.X);
        Canvas.SetLeft(_topLeft, Canvas.GetLeft(_topLeft) + e.Vector.X);
        Canvas.SetLeft(_bottomLeft, Canvas.GetLeft(_bottomLeft) + e.Vector.X);

        Canvas.SetTop(_bottom, Canvas.GetTop(_bottom) + e.Vector.Y);
        Canvas.SetTop(_bottomLeft, Canvas.GetTop(_bottomLeft) + e.Vector.Y);
        Canvas.SetTop(_bottomRight, Canvas.GetTop(_bottomRight) + e.Vector.Y);

        var rect = GetRect();

        Canvas.SetLeft(_top, rect.Center.X);
        Canvas.SetLeft(_bottom, rect.Center.X);

        Canvas.SetTop(_left, rect.Center.Y);
        Canvas.SetTop(_right, rect.Center.Y);

        UpdateDrag(rect);


        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }

    private void OnDragDeltaBottomRight(object? sender, VectorEventArgs e)
    {
        Canvas.SetLeft(_right, Canvas.GetLeft(_right) + e.Vector.X);
        Canvas.SetLeft(_topRight, Canvas.GetLeft(_topRight) + e.Vector.X);
        Canvas.SetLeft(_bottomRight, Canvas.GetLeft(_bottomRight) + e.Vector.X);

        Canvas.SetTop(_bottom, Canvas.GetTop(_bottom) + e.Vector.Y);
        Canvas.SetTop(_bottomLeft, Canvas.GetTop(_bottomLeft) + e.Vector.Y);
        Canvas.SetTop(_bottomRight, Canvas.GetTop(_bottomRight) + e.Vector.Y);

        var rect = GetRect();

        Canvas.SetLeft(_top, rect.Center.X);
        Canvas.SetLeft(_bottom, rect.Center.X);

        Canvas.SetTop(_left, rect.Center.Y);
        Canvas.SetTop(_right, rect.Center.Y);

        UpdateDrag(rect);


        if (ControlCrent is { })
        {
            UpdateControl(ControlCrent, rect.Inflate(new Thickness(-_leftOffset, -_topOffset, _leftOffset, _topOffset)));
        }
    }
    protected override Size MeasureOverride(Size availableSize)
    {
        var rect = Rect;

        foreach (var visualChild in VisualChildren)
        {
            if (visualChild is Control control)
            {
                control.Measure(rect.Size);
            }
        }

        return rect.Size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = Rect;

        foreach (var visualChild in VisualChildren)
        {
            if (visualChild is Control control)
            {
                control.Arrange(rect);
            }
        }

        return rect.Size;
    }
}
