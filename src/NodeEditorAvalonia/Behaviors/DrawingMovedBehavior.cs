using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingMovedBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<DrawingMovedBehavior, IDrawingNode?>(nameof(DrawingSource));

    public static readonly StyledProperty<Control?> InputSourceProperty =
        AvaloniaProperty.Register<DrawingMovedBehavior, Control?>(nameof(InputSource));

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

        _inputSource.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
    }

    private void DeInitialize()
    {
        if (_inputSource is null)
        {
            return;
        }

        _inputSource.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        _inputSource = null;
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (drawingNode.Settings.EnableInk && drawingNode.Settings.IsInkMode)
        {
            return;
        }

        var (x, y) = e.GetPosition(AssociatedObject ?? _inputSource);

        drawingNode.ConnectorMove(x, y);
    }
}
