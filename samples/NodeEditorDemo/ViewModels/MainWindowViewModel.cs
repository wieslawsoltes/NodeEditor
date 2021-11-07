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
            _serializer = new NodeSerializer(typeof(ObservableCollection<>));
            _factory = new();

            _templates = _factory.CreateTemplates();

            Drawing = _factory.CreateDemoDrawing();
            Drawing.Serializer = _serializer;

            NewCommand = ReactiveCommand.Create(New);

            OpenCommand = ReactiveCommand.CreateFromTask(async () => await Open());

            SaveCommand = ReactiveCommand.CreateFromTask(async () => await Save());
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

        private void New()
        {
            Drawing = _factory.CreateDrawing();
            Drawing.Serializer = _serializer;
        }

        private async Task Open()
        {
            var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window is null)
            {
                return;
            }
            var dlg = new OpenFileDialog { AllowMultiple = false };
            dlg.Filters.Add(new FileDialogFilter { Name = "Json Files (*.json)", Extensions = new List<string> { "json" } });
            dlg.Filters.Add(new FileDialogFilter { Name = "All Files (*.*)", Extensions = new List<string> { "*" } });
            var result = await dlg.ShowAsync(window);
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
            var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window is null)
            {
                return;
            }
            var dlg = new SaveFileDialog();
            dlg.Filters.Add(new FileDialogFilter { Name = "Json Files (*.json)", Extensions = new List<string> { "json" } });
            dlg.Filters.Add(new FileDialogFilter { Name = "All Files (*.*)", Extensions = new List<string> { "*" } });
            dlg.InitialFileName = System.IO.Path.GetFileNameWithoutExtension("drawing");
            var result = await dlg.ShowAsync(window);
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
