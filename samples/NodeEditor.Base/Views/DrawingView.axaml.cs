using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NodeEditorDemo.Controls;

namespace NodeEditorDemo.Views;

public class DrawingView : UserControl
{
    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty = 
        AvaloniaProperty.Register<DrawingView, NodeZoomBorder?>(nameof(ZoomControl));

    public DrawingView()
    {
        InitializeComponent();
    }

    public NodeZoomBorder? ZoomControl
    {
        get => GetValue(ZoomControlProperty);
        set => SetValue(ZoomControlProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
