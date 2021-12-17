using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NodeEditorDemo.ViewModels;
using NodeEditorDemo.Views;

namespace NodeEditorDemo;

public class App : Application
{
    public static bool EnableInputOutput { get; set; } = true;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel
                {
                    IsEditMode = true,
                    IsToolboxVisible = true
                }
            };
        }
            
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            singleViewLifetime.MainView = new MainView
            {
                DataContext = new MainWindowViewModel
                {
                    IsEditMode = true,
                    IsToolboxVisible = false
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
