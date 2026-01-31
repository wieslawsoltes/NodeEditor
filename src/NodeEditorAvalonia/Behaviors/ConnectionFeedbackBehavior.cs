using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class ConnectionFeedbackBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<ConnectionFeedbackBehavior, IDrawingNode?>(nameof(DrawingSource));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty =
        AvaloniaProperty.Register<ConnectionFeedbackBehavior, Canvas?>(nameof(AdornerCanvas));

    public static readonly StyledProperty<IBrush?> RejectionStrokeProperty =
        AvaloniaProperty.Register<ConnectionFeedbackBehavior, IBrush?>(nameof(RejectionStroke));

    public static readonly StyledProperty<IBrush?> LabelBackgroundProperty =
        AvaloniaProperty.Register<ConnectionFeedbackBehavior, IBrush?>(nameof(LabelBackground));

    public static readonly StyledProperty<IBrush?> LabelBorderBrushProperty =
        AvaloniaProperty.Register<ConnectionFeedbackBehavior, IBrush?>(nameof(LabelBorderBrush));

    public static readonly StyledProperty<IBrush?> LabelForegroundProperty =
        AvaloniaProperty.Register<ConnectionFeedbackBehavior, IBrush?>(nameof(LabelForeground));

    public static readonly StyledProperty<string?> LabelTextProperty =
        AvaloniaProperty.Register<ConnectionFeedbackBehavior, string?>(nameof(LabelText));

    private const double StrokeThickness = 1.5;
    private static readonly IBrush DefaultRejectionBrush =
        new ImmutableSolidColorBrush(Color.FromArgb(0xFF, 0xE3, 0x3F, 0x2D));
    private static readonly IBrush DefaultLabelForeground =
        new ImmutableSolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xE6, 0xE6));
    private static readonly IBrush DefaultLabelBackground =
        new ImmutableSolidColorBrush(Color.FromArgb(0xCC, 0x1E, 0x1E, 0x1E));
    private const string DefaultLabelText = "Connection blocked";

    private IDisposable? _drawingSubscription;
    private IDrawingNode? _drawingNode;
    private GuidesAdorner? _feedbackAdorner;
    private Border? _feedbackLabel;
    private TextBlock? _feedbackLabelText;
    private DispatcherTimer? _dismissTimer;
    private Canvas? _adornerCanvas;

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    public Canvas? AdornerCanvas
    {
        get => GetValue(AdornerCanvasProperty);
        set => SetValue(AdornerCanvasProperty, value);
    }

    public IBrush? RejectionStroke
    {
        get => GetValue(RejectionStrokeProperty);
        set => SetValue(RejectionStrokeProperty, value);
    }

    public IBrush? LabelBackground
    {
        get => GetValue(LabelBackgroundProperty);
        set => SetValue(LabelBackgroundProperty, value);
    }

    public IBrush? LabelBorderBrush
    {
        get => GetValue(LabelBorderBrushProperty);
        set => SetValue(LabelBorderBrushProperty, value);
    }

    public IBrush? LabelForeground
    {
        get => GetValue(LabelForegroundProperty);
        set => SetValue(LabelForegroundProperty, value);
    }

    public string? LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RejectionStrokeProperty
            || change.Property == LabelBackgroundProperty
            || change.Property == LabelBorderBrushProperty
            || change.Property == LabelForegroundProperty
            || change.Property == LabelTextProperty)
        {
            ApplyTheme();
        }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        _drawingSubscription = this
            .GetObservable(DrawingSourceProperty)
            .Subscribe(new AnonymousObserver<IDrawingNode?>(
                drawingNode =>
                {
                    if (_drawingNode is not null)
                    {
                        _drawingNode.ConnectionRejected -= OnConnectionRejected;
                    }

                    _drawingNode = drawingNode;
                    ClearFeedback();

                    if (_drawingNode is not null)
                    {
                        _drawingNode.ConnectionRejected += OnConnectionRejected;
                    }
                }));
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (_drawingNode is not null)
        {
            _drawingNode.ConnectionRejected -= OnConnectionRejected;
            _drawingNode = null;
        }

        _drawingSubscription?.Dispose();
        _drawingSubscription = null;

        StopTimer();
        ClearFeedback();
    }

    private void OnConnectionRejected(object? sender, ConnectionRejectedEventArgs e)
    {
        var layer = AdornerCanvas;
        if (layer is null)
        {
            return;
        }

        _adornerCanvas = layer;

        var start = GetPinPoint(e.Start);
        var end = GetPinPoint(e.End);

        ShowFeedback(start, end);
    }

    private void ShowFeedback(Point start, Point end)
    {
        var layer = _adornerCanvas;
        if (layer is null)
        {
            return;
        }

        if (_feedbackAdorner is null)
        {
            _feedbackAdorner = new GuidesAdorner
            {
                StrokeThickness = StrokeThickness,
                IsHitTestVisible = false
            };
            layer.Children.Add(_feedbackAdorner);
        }

        _feedbackAdorner.Guides = new List<GuideLine> { new(start, end) };

        if (_feedbackLabel is null)
        {
            _feedbackLabelText = new TextBlock
            {
                FontSize = 12
            };

            _feedbackLabel = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                Child = _feedbackLabelText,
                IsHitTestVisible = false
            };

            _feedbackLabel.SetValue(Panel.ZIndexProperty, 10);
            layer.Children.Add(_feedbackLabel);
        }

        ApplyTheme();

        _feedbackLabelText!.Text = string.IsNullOrWhiteSpace(LabelText) ? DefaultLabelText : LabelText;
        Canvas.SetLeft(_feedbackLabel, end.X + 8.0);
        Canvas.SetTop(_feedbackLabel, end.Y + 8.0);

        StartTimer();
    }

    private void ApplyTheme()
    {
        if (_feedbackAdorner is not null)
        {
            _feedbackAdorner.Stroke = RejectionStroke ?? DefaultRejectionBrush;
        }

        if (_feedbackLabel is not null)
        {
            _feedbackLabel.Background = LabelBackground ?? DefaultLabelBackground;
            _feedbackLabel.BorderBrush = LabelBorderBrush ?? RejectionStroke ?? DefaultRejectionBrush;
        }

        if (_feedbackLabelText is not null)
        {
            _feedbackLabelText.Foreground = LabelForeground ?? DefaultLabelForeground;
        }
    }

    private void ClearFeedback()
    {
        var layer = _adornerCanvas;
        if (layer is null)
        {
            return;
        }

        if (_feedbackAdorner is not null)
        {
            layer.Children.Remove(_feedbackAdorner);
            _feedbackAdorner = null;
        }

        if (_feedbackLabel is not null)
        {
            layer.Children.Remove(_feedbackLabel);
            _feedbackLabel = null;
            _feedbackLabelText = null;
        }
    }

    private void StartTimer()
    {
        if (_dismissTimer is null)
        {
            _dismissTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(650)
            };
            _dismissTimer.Tick += DismissTimerTick;
        }

        _dismissTimer.Stop();
        _dismissTimer.Start();
    }

    private void StopTimer()
    {
        if (_dismissTimer is null)
        {
            return;
        }

        _dismissTimer.Stop();
        _dismissTimer.Tick -= DismissTimerTick;
        _dismissTimer = null;
    }

    private void DismissTimerTick(object? sender, EventArgs e)
    {
        _dismissTimer?.Stop();
        ClearFeedback();
    }

    private static Point GetPinPoint(IPin pin)
    {
        var x = pin.X;
        var y = pin.Y;

        if (pin.Parent is not INode parent)
        {
            return new Point(x, y);
        }

        if (Math.Abs(parent.Rotation) > 0.001)
        {
            var centerX = parent.Width / 2.0;
            var centerY = parent.Height / 2.0;
            var radians = parent.Rotation * Math.PI / 180.0;
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            var dx = x - centerX;
            var dy = y - centerY;
            var rotatedX = dx * cos - dy * sin + centerX;
            var rotatedY = dx * sin + dy * cos + centerY;
            x = rotatedX;
            y = rotatedY;
        }

        return new Point(x + parent.X, y + parent.Y);
    }
}
