using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NodeEditorDemo.Controls;

namespace NodeEditorDemo.Views;

public class MenuView : UserControl
{
    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty = 
        AvaloniaProperty.Register<MenuView, NodeZoomBorder?>(nameof(ZoomControl));

    public MenuView()
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
