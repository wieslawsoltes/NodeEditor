using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls;

public class DrawingNode : TemplatedControl
{
    public static readonly AttachedProperty<bool> IsEditModeProperty = 
        AvaloniaProperty.RegisterAttached<IAvaloniaObject, bool>("IsEditMode", typeof(DrawingNode), true, true);

    public static readonly StyledProperty<Control?> InputSourceProperty = 
        AvaloniaProperty.Register<DrawingNode, Control?>(nameof(InputSource));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty = 
        AvaloniaProperty.Register<DrawingNode, Canvas?>(nameof(AdornerCanvas));

    public static readonly StyledProperty<bool> EnableSnapProperty = 
        AvaloniaProperty.Register<DrawingNode, bool>(nameof(EnableSnap));

    public static readonly StyledProperty<double> SnapXProperty = 
        AvaloniaProperty.Register<DrawingNode, double>(nameof(SnapX), 1.0);

    public static readonly StyledProperty<double> SnapYProperty = 
        AvaloniaProperty.Register<DrawingNode, double>(nameof(SnapY), 1.0);

    public Control? InputSource
    {
        get => GetValue(InputSourceProperty);
        set => SetValue(InputSourceProperty, value);
    }

    public Canvas? AdornerCanvas
    {
        get => GetValue(AdornerCanvasProperty);
        set => SetValue(AdornerCanvasProperty, value);
    }

    public bool EnableSnap
    {
        get => GetValue(EnableSnapProperty);
        set => SetValue(EnableSnapProperty, value);
    }

    public double SnapX
    {
        get => GetValue(SnapXProperty);
        set => SetValue(SnapXProperty, value);
    }

    public double SnapY
    {
        get => GetValue(SnapYProperty);
        set => SetValue(SnapYProperty, value);
    }

    public static bool GetIsEditMode(IAvaloniaObject obj)
    {
        return obj.GetValue(IsEditModeProperty);
    }

    public static void SetIsEditMode(IAvaloniaObject obj, bool value)
    {
        obj.SetValue(IsEditModeProperty, value);
    }
}
