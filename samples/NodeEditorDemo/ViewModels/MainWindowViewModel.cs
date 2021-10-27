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
using NodeEditor.ViewModels;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<NodeTemplateViewModel>? _templates;
        private DrawingNodeViewModel? _drawing;

        public MainWindowViewModel()
        {
            _templates = new ObservableCollection<NodeTemplateViewModel>
            {
                new()
                {
                    Title = "Rectangle",
                    Build = (x, y) => NodeFactory.CreateRectangle(x, y, 60, 60, "rect")
                },
                new()
                {
                    Title = "Ellipse",
                    Build = (x, y) => NodeFactory.CreateEllipse(x, y, 60, 60, "ellipse")
                },
                new()
                {
                    Title = "Signal",
                    Build = (x, y) => NodeFactory.CreateSignal(x, y, label: "signal", state: false)
                },
                new()
                {
                    Title = "AND Gate",
                    Build = (x, y) => NodeFactory.CreateAndGate(x, y, 30, 30)
                },
                new()
                {
                    Title = "OR Gate",
                    Build = (x, y) => NodeFactory.CreateOrGate(x, y, 30, 30)
                },
            };

            Drawing = NodeFactory.CreateDemoDrawing();

            NewCommand = ReactiveCommand.Create(() =>
            {
                Drawing = NodeFactory.CreateDrawing();
            });

            OpenCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dlg = new OpenFileDialog { AllowMultiple = false };
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json Files (*.json)", Extensions = new List<string> { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All Files (*.*)", Extensions = new List<string> { "*" } });
                var result = await dlg.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
                if (result is { } && result.Length == 1)
                {
                    try
                    {
                        var json = await Task.Run(() =>  System.IO.File.ReadAllText(result.First()));
                        var drawing = NodeSerializer.Deserialize<DrawingNodeViewModel?>(json);
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
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json Files (*.json)", Extensions = new List<string> { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All Files (*.*)", Extensions = new List<string> { "*" } });
                dlg.InitialFileName = System.IO.Path.GetFileNameWithoutExtension("drawing");
                var result = await dlg.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
                if (result is { })
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            var json = NodeSerializer.Serialize(_drawing);
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

        public ObservableCollection<NodeTemplateViewModel>? Templates
        {
            get => _templates;
            set => this.RaiseAndSetIfChanged(ref _templates, value);
        }

        public DrawingNodeViewModel? Drawing
        {
            get => _drawing;
            set => this.RaiseAndSetIfChanged(ref _drawing, value);
        }

        public ICommand NewCommand { get; }

        public ICommand OpenCommand { get; }

        public ICommand SaveCommand { get; }
    }
}
