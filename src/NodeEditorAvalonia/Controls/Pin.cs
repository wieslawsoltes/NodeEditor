using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class Pin : TemplatedControl
{
    public static readonly StyledProperty<IPin?> PinSourceProperty =
        AvaloniaProperty.Register<Pin, IPin?>(nameof(PinSource));

    public static readonly StyledProperty<PinAlignment> AlignmentProperty =
        AvaloniaProperty.Register<Pin, PinAlignment>(nameof(Alignment));

    public static readonly StyledProperty<PinDirection> DirectionProperty =
        AvaloniaProperty.Register<Pin, PinDirection>(nameof(Direction), PinDirection.Bidirectional);

    public static readonly StyledProperty<int> BusWidthProperty =
        AvaloniaProperty.Register<Pin, int>(nameof(BusWidth), 1);

    public static readonly StyledProperty<string?> IdProperty =
        AvaloniaProperty.Register<Pin, string?>(nameof(Id));

    public IPin? PinSource
    {
        get => GetValue(PinSourceProperty);
        set => SetValue(PinSourceProperty, value);
    }

    public PinAlignment Alignment
    {
        get => GetValue(AlignmentProperty);
        set => SetValue(AlignmentProperty, value);
    }

    public PinDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public int BusWidth
    {
        get => GetValue(BusWidthProperty);
        set => SetValue(BusWidthProperty, value);
    }

    public string? Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var pin = PinSource ?? DataContext as IPin;
        if (pin is null)
        {
            return;
        }

        if (pin.Parent is not { } nodeViewModel)
        {
            return;
        }

        if (nodeViewModel.Parent is IDrawingNode drawingNode)
        {
            var info = e.GetCurrentPoint(this);
            var isPrimary = info.Properties.IsLeftButtonPressed || info.Pointer.Type != PointerType.Mouse;
            if (!isPrimary)
            {
                return;
            }

            if (!drawingNode.CanConnectPin(pin) || !pin.CanConnect())
            {
                return;
            }

            drawingNode.ConnectorLeftPressed(pin, showWhenMoving: true);
            e.Handled = true;
        }
    }
}
