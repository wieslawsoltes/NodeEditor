using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;
using NodeEditor.Controls;

namespace NodeEditor.Behaviors;

public class DrawingPressedBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<DrawingPressedBehavior, IDrawingNode?>(nameof(DrawingSource));

    public static readonly StyledProperty<Control?> InputSourceProperty =
        AvaloniaProperty.Register<DrawingPressedBehavior, Control?>(nameof(InputSource));

    private Control? _inputSource;

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    public Control? InputSource
    {
        get => GetValue(InputSourceProperty);
        set => SetValue(InputSourceProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == InputSourceProperty)
        {
            DeInitialize();

            if (AssociatedObject is not null)
            {
                Initialize();
            }
        }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            Initialize();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        DeInitialize();
    }

    private void Initialize()
    {
        _inputSource = InputSource ?? AssociatedObject;
        if (_inputSource is null)
        {
            return;
        }

        _inputSource.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
    }

    private void DeInitialize()
    {
        if (_inputSource is null)
        {
            return;
        }

        _inputSource.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        _inputSource = null;
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (drawingNode.Settings.EnableInk && drawingNode.Settings.IsInkMode)
        {
            return;
        }

        if (IsPinSource(e.Source))
        {
            return;
        }

        var info = e.GetCurrentPoint(_inputSource);
        var (x, y) = e.GetPosition(AssociatedObject ?? _inputSource);

        if (info.Properties.IsLeftButtonPressed)
        {
            drawingNode.DrawingLeftPressed(x, y);
        }
        else if (info.Properties.IsRightButtonPressed)
        {
            drawingNode.DrawingRightPressed(x, y);
        }
    }

    private static bool IsPinSource(object? source)
    {
        if (source is not Visual visual)
        {
            return false;
        }

        if (visual is Pin)
        {
            return true;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is Pin)
            {
                return true;
            }
        }

        return false;
    }
}
