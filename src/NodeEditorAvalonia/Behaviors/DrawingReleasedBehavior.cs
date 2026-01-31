using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingReleasedBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<DrawingReleasedBehavior, IDrawingNode?>(nameof(DrawingSource));

    public static readonly StyledProperty<double> PinHitToleranceProperty =
        AvaloniaProperty.Register<DrawingReleasedBehavior, double>(nameof(PinHitTolerance), 8.0);

    public static readonly StyledProperty<Control?> InputSourceProperty =
        AvaloniaProperty.Register<DrawingReleasedBehavior, Control?>(nameof(InputSource));

    private Control? _inputSource;

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    public double PinHitTolerance
    {
        get => GetValue(PinHitToleranceProperty);
        set => SetValue(PinHitToleranceProperty, value);
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

        _inputSource.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
    }

    private void DeInitialize()
    {
        if (_inputSource is null)
        {
            return;
        }

        _inputSource.RemoveHandler(InputElement.PointerReleasedEvent, Released);
        _inputSource = null;
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (drawingNode.Settings.EnableInk && drawingNode.Settings.IsInkMode)
        {
            return;
        }

        if (!drawingNode.IsConnectorMoving())
        {
            return;
        }

        var isPrimary = e.InitialPressMouseButton == MouseButton.Left || e.Pointer.Type != PointerType.Mouse;
        if (!isPrimary)
        {
            return;
        }

        var position = e.GetPosition(AssociatedObject ?? _inputSource);
        if (HitTestHelper.TryFindPinAtPoint(drawingNode, position, PinHitTolerance, out var pin) && pin is not null)
        {
            drawingNode.ConnectorLeftPressed(pin, showWhenMoving: true);
        }
        else
        {
            drawingNode.CancelConnector();
        }

        if (drawingNode is IUndoRedoHost host)
        {
            host.EndUndoBatch();
        }

        e.Handled = true;
    }
}
