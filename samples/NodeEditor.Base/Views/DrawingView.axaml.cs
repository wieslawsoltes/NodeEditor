using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NodeEditor.Controls;
using NodeEditorDemo.Controls;

namespace NodeEditorDemo.Views;

public class DrawingView : UserControl
{
    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty = 
        AvaloniaProperty.Register<DrawingView, NodeZoomBorder?>(nameof(ZoomControl));

    public static readonly StyledProperty<DrawingNode?> DrawingNodeProperty = 
        AvaloniaProperty.Register<DrawingView, DrawingNode?>(nameof(DrawingNode));

    public DrawingView()
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
