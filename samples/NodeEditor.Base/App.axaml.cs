using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NodeEditorDemo.ViewModels;
using NodeEditorDemo.Views;

namespace NodeEditorDemo;

public class App : Application
{
    public static bool EnableInputOutput { get; set; } = true;

    public static bool EnableMainMenu { get; set; } = true;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public App()
    {
        Name = "NodeEditor";
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new EditorViewModel
            {
                IsEditMode = true,
                IsToolboxVisible = true
            };

            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };

            DataContext = vm;
        }
            
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            var vm = new EditorViewModel
            {
                IsEditMode = true,
                IsToolboxVisible = false
            };
            singleViewLifetime.MainView = new MainView
            {
                DataContext = vm
            };

            DataContext = vm;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
