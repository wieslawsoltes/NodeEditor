using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NodeEditor.Mvvm;
using NodeEditorDemo.Services;
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
        var vm = new MainViewViewModel
        {
            IsToolboxVisible = true
        };
        
        var editor = new EditorViewModel
        {
            Serializer = new NodeSerializer(typeof(ObservableCollection<>)),
            Factory = new NodeFactory()
        };

        editor.Templates = editor.Factory.CreateTemplates();
        editor.Drawing = Demo.CreateDemoDrawing();
        editor.Drawing.SetSerializer(editor.Serializer);

        vm.Editor = editor;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };

            DataContext = vm;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            singleViewLifetime.MainView = new MainView
            {
                DataContext = vm
            };

            DataContext = vm;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
