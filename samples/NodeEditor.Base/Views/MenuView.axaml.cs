using Avalonia;
using Avalonia.Controls;
using NodeEditor.Controls;

namespace NodeEditorDemo.Views;

public partial class MenuView : UserControl
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
}
