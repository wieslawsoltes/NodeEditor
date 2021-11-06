using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NodeEditor.Model;
using NodeEditor.Serializer;
using NodeEditor.ViewModels;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly INodeSerializer _serializer = new NodeSerializer(typeof(ObservableCollection<>));
        private readonly NodeFactory _factory = new();
        private ObservableCollection<INodeTemplate>? _templates;
        private IDrawingNode? _drawing;

        public MainWindowViewModel()
        {
            _templates = new ObservableCollection<INodeTemplate>
            {
                new NodeTemplateViewModel
                {
                    Title = "Rectangle",
                    Build = (x, y) => _factory.CreateRectangle(x, y, 60, 60, "rect")
                },
                new NodeTemplateViewModel
                {
                    Title = "Ellipse",
                    Build = (x, y) => _factory.CreateEllipse(x, y, 60, 60, "ellipse")
                },
                new NodeTemplateViewModel
                {
                    Title = "Signal",
                    Build = (x, y) => _factory.CreateSignal(x, y, label: "signal", state: false)
                },
                new NodeTemplateViewModel
                {
                    Title = "AND Gate",
                    Build = (x, y) => _factory.CreateAndGate(x, y, 30, 30)
                },
                new NodeTemplateViewModel
                {
                    Title = "OR Gate",
                    Build = (x, y) => _factory.CreateOrGate(x, y, 30, 30)
                },
            };

            Drawing = _factory.CreateDemoDrawing();

            NewCommand = ReactiveCommand.Create(() =>
            {
                Drawing = _factory.CreateDrawing();
            });

            OpenCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dlg = new OpenFileDialog { AllowMultiple = false };
                dlg.Filters.Add(new FileDialogFilter { Name = "Json Files (*.json)", Extensions = new List<string> { "json" } });
                dlg.Filters.Add(new FileDialogFilter { Name = "All Files (*.*)", Extensions = new List<string> { "*" } });
                var result = await dlg.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
                if (result is { Length: 1 })
                {
                    try
                    {
                        var json = await Task.Run(() =>  System.IO.File.ReadAllText(result.First()));
                        var drawing = _serializer.Deserialize<DrawingNodeViewModel?>(json);
                        if (drawing is { })
                        {
                            Drawing = drawing;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                    }
                }
            });

            SaveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dlg = new SaveFileDialog();
                dlg.Filters.Add(new FileDialogFilter { Name = "Json Files (*.json)", Extensions = new List<string> { "json" } });
                dlg.Filters.Add(new FileDialogFilter { Name = "All Files (*.*)", Extensions = new List<string> { "*" } });
                dlg.InitialFileName = System.IO.Path.GetFileNameWithoutExtension("drawing");
                var result = await dlg.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
                if (result is { })
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            var json = _serializer.Serialize(_drawing);
                            System.IO.File.WriteAllText(result, json);
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                    }
                }
            });
        }

        public ObservableCollection<INodeTemplate>? Templates
        {
            get => _templates;
            set => this.RaiseAndSetIfChanged(ref _templates, value);
        }

        public IDrawingNode? Drawing
        {
            get => _drawing;
            set => this.RaiseAndSetIfChanged(ref _drawing, value);
        }

        public ICommand NewCommand { get; }

        public ICommand OpenCommand { get; }

        public ICommand SaveCommand { get; }
    }
}
