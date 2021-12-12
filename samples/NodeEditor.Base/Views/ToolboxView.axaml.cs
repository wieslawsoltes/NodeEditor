using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NodeEditor.Controls;
using NodeEditorDemo.Controls;

namespace NodeEditorDemo.Views;

public class ToolboxView : UserControl
{
    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty = 
        AvaloniaProperty.Register<ToolboxView, NodeZoomBorder?>(nameof(ZoomControl));

    public static readonly StyledProperty<DrawingNode?> DrawingNodeProperty = 
        AvaloniaProperty.Register<ToolboxView, DrawingNode?>(nameof(DrawingNode));

    public ToolboxView()
    {
        InitializeComponent();
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
