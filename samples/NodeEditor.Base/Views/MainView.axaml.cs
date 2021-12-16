using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditorDemo.Views;

public class MainView : UserControl
{
    public static readonly StyledProperty<bool> IsMenuViewVisibleProperty = 
        AvaloniaProperty.Register<MainView, bool>(nameof(IsMenuViewVisible));
    
    public static readonly StyledProperty<bool> IsToolboxViewVisibleProperty = 
        AvaloniaProperty.Register<MainView, bool>(nameof(IsToolboxViewVisible));

    public MainView()
    {
        InitializeComponent();
    }

    public bool IsMenuViewVisible
    {
        get => GetValue(IsMenuViewVisibleProperty);
        set => SetValue(IsMenuViewVisibleProperty, value);
    }

    public bool IsToolboxViewVisible
    {
        get => GetValue(IsToolboxViewVisibleProperty);
        set => SetValue(IsToolboxViewVisibleProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
