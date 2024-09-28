using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class PinPressedBehavior : Behavior<ContentPresenter>
{
    public static readonly StyledProperty<IPin?> PinSourceProperty =
        AvaloniaProperty.Register<PinPressedBehavior, IPin?>(nameof(PinSource));

    public IPin? PinSource
    {
        get => GetValue(PinSourceProperty);
        set => SetValue(PinSourceProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is not null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        }
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (AssociatedObject?.DataContext is not IPin pin)
        {
            return;
        }

        if (pin.Parent is not { } nodeViewModel)
        {
            return;
        }

        if (nodeViewModel.Parent is IDrawingNode drawingNode)
        {
            var info = e.GetCurrentPoint(AssociatedObject);

            if (info.Properties.IsLeftButtonPressed)
            {
                var showWhenMoving = info.Pointer.Type == PointerType.Mouse;

                drawingNode.ConnectorLeftPressed(pin, showWhenMoving);

                e.Handled = true;
            }
        }
    }
}
