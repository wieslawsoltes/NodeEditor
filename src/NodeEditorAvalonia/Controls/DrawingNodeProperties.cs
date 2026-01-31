using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class DrawingNodeProperties : TemplatedControl
{
    public static readonly StyledProperty<bool> IsInkModeProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(IsInkMode), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> EnableConnectionsProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(EnableConnections), true, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> RequireDirectionalConnectionsProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(RequireDirectionalConnections), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> RequireMatchingBusWidthProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(RequireMatchingBusWidth), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> EnableMultiplePinConnectionsProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(EnableMultiplePinConnections), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> AllowSelfConnectionsProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(AllowSelfConnections), true, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> AllowDuplicateConnectionsProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(AllowDuplicateConnections), true, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> EnableSnapProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(EnableSnap), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> SnapXProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(SnapX), 1.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> SnapYProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(SnapY), 1.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> NudgeStepProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(NudgeStep), 1.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> NudgeMultiplierProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(NudgeMultiplier), 10.0, false, BindingMode.TwoWay);
    
    public static readonly StyledProperty<bool> EnableGridProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(EnableGrid), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> GridCellWidthProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(GridCellWidth), 15.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> GridCellHeightProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(GridCellHeight), 15.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> EnableGuidesProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(EnableGuides), true, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> GuideSnapToleranceProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(GuideSnapTolerance), 6.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<bool> EnableConnectorRoutingProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, bool>(nameof(EnableConnectorRouting), true, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> RoutingGridSizeProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(RoutingGridSize), 10.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> RoutingObstaclePaddingProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(RoutingObstaclePadding), 8.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<ConnectorRoutingAlgorithm> RoutingAlgorithmProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, ConnectorRoutingAlgorithm>(nameof(RoutingAlgorithm), ConnectorRoutingAlgorithm.Auto, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> RoutingBendPenaltyProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(RoutingBendPenalty), 0.6, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> RoutingDiagonalCostProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(RoutingDiagonalCost), 1.4, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> RoutingCornerRadiusProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(RoutingCornerRadius), 10.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<int> RoutingMaxCellsProperty =
        AvaloniaProperty.Register<DrawingNodeProperties, int>(nameof(RoutingMaxCells), 200, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> DrawingWidthProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(DrawingWidth), 0.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> DrawingHeightProperty = 
        AvaloniaProperty.Register<DrawingNodeProperties, double>(nameof(DrawingHeight), 0.0, false, BindingMode.TwoWay);

    public bool IsInkMode
    {
        get => GetValue(IsInkModeProperty);
        set => SetValue(IsInkModeProperty, value);
    }

    public bool EnableConnections
    {
        get => GetValue(EnableConnectionsProperty);
        set => SetValue(EnableConnectionsProperty, value);
    }

    public bool RequireDirectionalConnections
    {
        get => GetValue(RequireDirectionalConnectionsProperty);
        set => SetValue(RequireDirectionalConnectionsProperty, value);
    }

    public bool RequireMatchingBusWidth
    {
        get => GetValue(RequireMatchingBusWidthProperty);
        set => SetValue(RequireMatchingBusWidthProperty, value);
    }

    public bool EnableMultiplePinConnections
    {
        get => GetValue(EnableMultiplePinConnectionsProperty);
        set => SetValue(EnableMultiplePinConnectionsProperty, value);
    }

    public bool AllowSelfConnections
    {
        get => GetValue(AllowSelfConnectionsProperty);
        set => SetValue(AllowSelfConnectionsProperty, value);
    }

    public bool AllowDuplicateConnections
    {
        get => GetValue(AllowDuplicateConnectionsProperty);
        set => SetValue(AllowDuplicateConnectionsProperty, value);
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

    public double NudgeStep
    {
        get => GetValue(NudgeStepProperty);
        set => SetValue(NudgeStepProperty, value);
    }

    public double NudgeMultiplier
    {
        get => GetValue(NudgeMultiplierProperty);
        set => SetValue(NudgeMultiplierProperty, value);
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

    public bool EnableGuides
    {
        get => GetValue(EnableGuidesProperty);
        set => SetValue(EnableGuidesProperty, value);
    }

    public double GuideSnapTolerance
    {
        get => GetValue(GuideSnapToleranceProperty);
        set => SetValue(GuideSnapToleranceProperty, value);
    }

    public bool EnableConnectorRouting
    {
        get => GetValue(EnableConnectorRoutingProperty);
        set => SetValue(EnableConnectorRoutingProperty, value);
    }

    public double RoutingGridSize
    {
        get => GetValue(RoutingGridSizeProperty);
        set => SetValue(RoutingGridSizeProperty, value);
    }

    public double RoutingObstaclePadding
    {
        get => GetValue(RoutingObstaclePaddingProperty);
        set => SetValue(RoutingObstaclePaddingProperty, value);
    }

    public ConnectorRoutingAlgorithm RoutingAlgorithm
    {
        get => GetValue(RoutingAlgorithmProperty);
        set => SetValue(RoutingAlgorithmProperty, value);
    }

    public double RoutingBendPenalty
    {
        get => GetValue(RoutingBendPenaltyProperty);
        set => SetValue(RoutingBendPenaltyProperty, value);
    }

    public double RoutingDiagonalCost
    {
        get => GetValue(RoutingDiagonalCostProperty);
        set => SetValue(RoutingDiagonalCostProperty, value);
    }

    public double RoutingCornerRadius
    {
        get => GetValue(RoutingCornerRadiusProperty);
        set => SetValue(RoutingCornerRadiusProperty, value);
    }

    public int RoutingMaxCells
    {
        get => GetValue(RoutingMaxCellsProperty);
        set => SetValue(RoutingMaxCellsProperty, value);
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
