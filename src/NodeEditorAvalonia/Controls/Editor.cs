using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls;

[TemplatePart("PART_ZoomBorder", typeof(NodeZoomBorder))]
[TemplatePart("PART_Drawing", typeof(DrawingNode))]
[TemplatePart("PART_AdornerCanvas", typeof(Canvas))]
public class Editor : TemplatedControl
{
    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty = 
        AvaloniaProperty.Register<Editor, NodeZoomBorder?>(nameof(ZoomControl));

    public static readonly StyledProperty<DrawingNode?> DrawingNodeProperty = 
        AvaloniaProperty.Register<Editor, DrawingNode?>(nameof(DrawingNode));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty = 
        AvaloniaProperty.Register<Editor, Canvas?>(nameof(AdornerCanvas));

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

    public Canvas? AdornerCanvas
    {
        get => GetValue(AdornerCanvasProperty);
        set => SetValue(AdornerCanvasProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ZoomControl = e.NameScope.Find<NodeZoomBorder>("PART_ZoomBorder");
        DrawingNode = e.NameScope.Find<DrawingNode>("PART_DrawingNode");
        AdornerCanvas = e.NameScope.Find<Canvas>("PART_AdornerCanvas");
    }
}
