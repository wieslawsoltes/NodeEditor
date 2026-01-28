using Avalonia;
using Avalonia.Controls;
using NodeEditor.Controls;

namespace NodeEditorLogic.Views;

public partial class MainView : UserControl
{
    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty =
        AvaloniaProperty.Register<MainView, NodeZoomBorder?>(nameof(MainView));

    public MainView()
    {
        InitializeComponent();
    }

    public NodeZoomBorder? ZoomControl
    {
        get => GetValue(ZoomControlProperty);
        set => SetValue(ZoomControlProperty, value);
    }
}
