using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NodeEditor.Controls;

namespace NodeEditorDemo.Views;

public class SettingsView : UserControl
{
    public static readonly StyledProperty<DrawingNode?> DrawingNodeProperty = 
        AvaloniaProperty.Register<SettingsView, DrawingNode?>(nameof(DrawingNode));

    public SettingsView()
    {
        InitializeComponent();
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
