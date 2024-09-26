using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace NodeEditor.Controls;

public class DrawingNodeProperties : TemplatedControl
{
    public static readonly StyledProperty<bool> EnableSnapProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(EnableSnap), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> SnapXProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(SnapX), 1.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> SnapYProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(SnapY), 1.0, false, BindingMode.TwoWay);
    
    public static readonly StyledProperty<bool> EnableGridProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(EnableGrid), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> GridCellWidthProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(GridCellWidth), 15.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> GridCellHeightProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(GridCellHeight), 15.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> DrawingWidthProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(DrawingWidth), 0.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> DrawingHeightProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(DrawingHeight), 0.0, false, BindingMode.TwoWay);

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

    public double DrawingWidth
    {
        get => GetValue(DrawingWidthProperty);
        set => SetValue(DrawingWidthProperty, value);
    }

    public double DrawingHeight
    {
        get => GetValue(DrawingHeightProperty);
        set => SetValue(DrawingHeightProperty, value);
    }
}
