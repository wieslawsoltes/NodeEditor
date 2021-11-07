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
        private readonly INodeSerializer _serializer;
        private readonly NodeFactory _factory;
        private IList<INodeTemplate>? _templates;
        private IDrawingNode? _drawing;

        public MainWindowViewModel()
        {
            CreateTemplates();

            _serializer = new NodeSerializer(typeof(ObservableCollection<>));
            _factory = new();

            Drawing = _factory.CreateDemoDrawing();
            Drawing.Serializer = _serializer;

            NewCommand = ReactiveCommand.Create(New);

            OpenCommand = ReactiveCommand.CreateFromTask(async () => await Open());

            SaveCommand = ReactiveCommand.CreateFromTask(async () => await Save());

            CutCommand = ReactiveCommand.Create(() => Drawing.CutNodes());

            CopyCommand = ReactiveCommand.Create(() => Drawing.CopyNodes());

            PasteCommand = ReactiveCommand.Create(() => Drawing.PasteNodes());
        }

        public IList<INodeTemplate>? Templates
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

        public ICommand CutCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand PasteCommand { get; }

        private void CreateTemplates()
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
                }
            };
        }

        private void New()
        {
            Drawing = _factory.CreateDrawing();
            Drawing.Serializer = _serializer;
        }

        private async Task Open()
        {
            var dlg = new OpenFileDialog { AllowMultiple = false };
            dlg.Filters.Add(new FileDialogFilter { Name = "Json Files (*.json)", Extensions = new List<string> { "json" } });
            dlg.Filters.Add(new FileDialogFilter { Name = "All Files (*.*)", Extensions = new List<string> { "*" } });
            var result =
                await dlg.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                    ?.MainWindow);
            if (result is { Length: 1 })
            {
                try
                {
                    var json = await Task.Run(() => System.IO.File.ReadAllText(result.First()));
                    var drawing = _serializer.Deserialize<DrawingNodeViewModel?>(json);
                    if (drawing is { })
                    {
                        Drawing = drawing;
                        Drawing.Serializer = _serializer;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        private async Task Save()
        {
            var dlg = new SaveFileDialog();
            dlg.Filters.Add(new FileDialogFilter { Name = "Json Files (*.json)", Extensions = new List<string> { "json" } });
            dlg.Filters.Add(new FileDialogFilter { Name = "All Files (*.*)", Extensions = new List<string> { "*" } });
            dlg.InitialFileName = System.IO.Path.GetFileNameWithoutExtension("drawing");
            var result =
                await dlg.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                    ?.MainWindow);
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
        }
    }
}
