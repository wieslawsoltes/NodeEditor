using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace NodeEditor.Controls;

[TemplatePart("PART_ZoomBorder", typeof(NodeZoomBorder))]
[TemplatePart("PART_Drawing", typeof(DrawingNode))]
[TemplatePart("PART_AdornerCanvas", typeof(Canvas))]
public class Editor : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> NodesSourceProperty =
        AvaloniaProperty.Register<Editor, IEnumerable?>(nameof(NodesSource));

    public static readonly StyledProperty<IEnumerable?> ConnectorsSourceProperty =
        AvaloniaProperty.Register<Editor, IEnumerable?>(nameof(ConnectorsSource));

    public static readonly StyledProperty<Control?> InputSourceProperty = 
        AvaloniaProperty.Register<Editor, Control?>(nameof(InputSource));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty = 
        AvaloniaProperty.Register<Editor, Canvas?>(nameof(AdornerCanvas));

    public static readonly StyledProperty<bool> EnableSnapProperty = 
        AvaloniaProperty.Register<Editor, bool>(nameof(EnableSnap), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> SnapXProperty = 
        AvaloniaProperty.Register<Editor, double>(nameof(SnapX), 1.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> SnapYProperty = 
        AvaloniaProperty.Register<Editor, double>(nameof(SnapY), 1.0, false, BindingMode.TwoWay);
    
    public static readonly StyledProperty<bool> EnableGridProperty = 
        AvaloniaProperty.Register<Editor, bool>(nameof(EnableGrid), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> GridCellWidthProperty = 
        AvaloniaProperty.Register<Editor, double>(nameof(GridCellWidth), 15.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> GridCellHeightProperty = 
        AvaloniaProperty.Register<Editor, double>(nameof(GridCellHeight), 15.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> DrawingWidthProperty = 
        AvaloniaProperty.Register<Editor, double>(nameof(DrawingWidth), 0.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<double> DrawingHeightProperty = 
        AvaloniaProperty.Register<Editor, double>(nameof(DrawingHeight), 0.0, false, BindingMode.TwoWay);

    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty = 
        AvaloniaProperty.Register<Editor, NodeZoomBorder?>(nameof(ZoomControl));

    public static readonly StyledProperty<DrawingNode?> DrawingNodeProperty = 
        AvaloniaProperty.Register<Editor, DrawingNode?>(nameof(DrawingNode));

    public IEnumerable? NodesSource
    {
        get => GetValue(NodesSourceProperty);
        set => SetValue(NodesSourceProperty, value);
    }

    public IEnumerable? ConnectorsSource
    {
        get => GetValue(ConnectorsSourceProperty);
        set => SetValue(ConnectorsSourceProperty, value);
    }

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

    public NodeZoomBorder? ZoomControl
    {
        get => GetValue(ZoomControlProperty);
        set => SetValue(ZoomControlProperty, value);
    }

    public DrawingNode? DrawingNode
    {
        get => GetValue(DrawingNodeProperty);
        set => SetValue(DrawingNodeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ZoomControl = e.NameScope.Find<NodeZoomBorder>("PART_ZoomBorder");
        DrawingNode = e.NameScope.Find<DrawingNode>("PART_DrawingNode");
        AdornerCanvas = e.NameScope.Find<Canvas>("PART_AdornerCanvas");
    }
}
