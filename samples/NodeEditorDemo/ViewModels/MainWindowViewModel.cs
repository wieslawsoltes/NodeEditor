using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NodeEditor.Controls;
using NodeEditor.Export;
using NodeEditor.Model;
using NodeEditor.Serializer;
using NodeEditor.ViewModels;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INodeTemplatesHost
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

            ExportCommand = ReactiveCommand.CreateFromTask(async control => await Export());

            ExitCommand = ReactiveCommand.Create(() =>
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
                {
                    desktopLifetime.Shutdown();
                }
            });
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

        public ICommand ExportCommand { get; }

        public ICommand ExitCommand { get; }

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
                    var json = await Task.Run(() => File.ReadAllText(result.First()));
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
            dlg.InitialFileName = Path.GetFileNameWithoutExtension("drawing");
            var result = await dlg.ShowAsync(window);
            if (result is { })
            {
                try
                {
                    await Task.Run(() =>
                    {
                        var json = _serializer.Serialize(_drawing);
                        File.WriteAllText(result, json);
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        public async Task Export()
        {
            if (Drawing is null)
            {
                return;
            }
            
            var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window is null)
            {
                return;
            }

            var dlg = new SaveFileDialog() { Title = "Save" };
            dlg.Filters.Add(new FileDialogFilter() { Name = "Png", Extensions = { "png" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Svg", Extensions = { "svg" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Pdf", Extensions = { "pdf" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.InitialFileName = Path.GetFileNameWithoutExtension("drawing");
            dlg.DefaultExtension = "png";

            var result = await dlg.ShowAsync(window);
            if (result is { } path)
            {
                var control = new DrawingNode
                {
                    DataContext = Drawing
                };
                
                var preview = new Window()
                {
                    Width = Drawing.Width,
                    Height = Drawing.Height,
                    Content = control,
                    ShowInTaskbar = false,
                    WindowState = WindowState.Minimized
                };

                preview.Show();

                var size = new Size(Drawing.Width, Drawing.Height);

                if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = File.Create(path);
                    PngRenderer.Render(preview, size, stream);
                }

                if (path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = File.Create(path);
                    SvgRenderer.Render(preview, size, stream);
                }

                if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = File.Create(path);
                    PdfRenderer.Render(preview, size, stream, 96);
                }
                
                preview.Close();
            }
        }
    }
}
