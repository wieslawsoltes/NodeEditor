using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NodeEditor.Mvvm;
using NodeEditorLogic.Services;
using NodeEditorLogic.ViewModels;
using NodeEditorLogic.Views;

namespace NodeEditorLogic;

public class App : Application
{
    public static bool EnableInputOutput { get; set; } = true;

    public static bool EnableMainMenu { get; set; } = true;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var library = new LogicComponentLibrary();
        var factory = new LogicNodeFactory(library);
        var simulation = new LogicSimulationService(library);

        var editor = new EditorViewModel
        {
            Serializer = new NodeSerializer(typeof(ObservableCollection<>)),
            Factory = factory
        };

        editor.Templates = editor.Factory.CreateTemplates();
        editor.Drawing = DemoCircuits.CreateDemoDrawing(factory);
        editor.Drawing.SetSerializer(editor.Serializer);

        var vm = new MainViewViewModel(simulation)
        {
            Editor = editor,
            IsToolboxVisible = true
        };

        vm.SetToolboxCategories(new ObservableCollection<ToolboxCategoryViewModel>(factory.CreateCategories()));

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
