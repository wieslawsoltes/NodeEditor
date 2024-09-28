using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using NodeEditor.Model;

namespace NodeEditor.Controls;

[TemplatePart("PART_ZoomBorder", typeof(NodeZoomBorder))]
[TemplatePart("PART_AdornerCanvas", typeof(Canvas))]
public class Editor : TemplatedControl
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<Editor, IDrawingNode?>(nameof(DrawingSource));
    
    public static readonly StyledProperty<Control?> InputSourceProperty = 
        AvaloniaProperty.Register<Editor, Control?>(nameof(InputSource));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty = 
        AvaloniaProperty.Register<Editor, Canvas?>(nameof(AdornerCanvas));

    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty = 
        AvaloniaProperty.Register<Editor, NodeZoomBorder?>(nameof(ZoomControl));

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
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

    public NodeZoomBorder? ZoomControl
    {
        get => GetValue(ZoomControlProperty);
        set => SetValue(ZoomControlProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ZoomControl = e.NameScope.Find<NodeZoomBorder>("PART_ZoomBorder");
        AdornerCanvas = e.NameScope.Find<Canvas>("PART_AdornerCanvas");
    }
}
