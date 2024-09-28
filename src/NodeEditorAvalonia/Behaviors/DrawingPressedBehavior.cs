using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingPressedBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<DrawingPressedBehavior, IDrawingNode?>(nameof(DrawingSource));

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
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

        if (DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (e.Source is Control { DataContext: IPin })
        {
            return;
        }

        var info = e.GetCurrentPoint(AssociatedObject);
        var (x, y) = e.GetPosition(AssociatedObject);

        if (info.Properties.IsLeftButtonPressed)
        {
            drawingNode.DrawingLeftPressed(x, y);
        }
        else if (info.Properties.IsRightButtonPressed)
        {
            drawingNode.DrawingRightPressed(x, y);
        }
    }
}
