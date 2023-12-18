using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls;

public class DrawingNode : TemplatedControl
{
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

    public static readonly StyledProperty<bool> EnableGridProperty = 
        AvaloniaProperty.Register<DrawingNode, bool>(nameof(EnableGrid));

    public static readonly StyledProperty<double> GridCellWidthProperty = 
        AvaloniaProperty.Register<DrawingNode, double>(nameof(GridCellWidth));

    public static readonly StyledProperty<double> GridCellHeightProperty = 
        AvaloniaProperty.Register<DrawingNode, double>(nameof(GridCellHeight));

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

    public bool EnableGrid
    {
        get => GetValue(EnableGridProperty);
        set => SetValue(EnableGridProperty, value);
    }

    public double GridCellWidth
    {
        get => GetValue(GridCellWidthProperty);
        set => SetValue(GridCellWidthProperty, value);
    }

    public double GridCellHeight
    {
        get => GetValue(GridCellHeightProperty);
        set => SetValue(GridCellHeightProperty, value);
    }
}
