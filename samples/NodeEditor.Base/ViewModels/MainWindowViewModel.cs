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
using NodeEditor.Export.Renderers;
using NodeEditor.Model;
using NodeEditor.Serializer;
using NodeEditor.ViewModels;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels;

public class MainWindowViewModel : ViewModelBase, INodeTemplatesHost
{
    private readonly INodeSerializer _serializer;
    private readonly NodeFactory _factory;
    private IList<INodeTemplate>? _templates;
    private IDrawingNode? _drawing;
    private bool _isEditMode;
    private bool _isToolboxVisible;

    public MainWindowViewModel()
    {
        _serializer = new NodeSerializer(typeof(ObservableCollection<>));
        _factory = new();

        _templates = _factory.CreateTemplates();

        Drawing = _factory.CreateDemoDrawing();
        Drawing.Serializer = _serializer;

        _isEditMode = true;
        _isToolboxVisible = true;

        ToggleEditModeCommand = ReactiveCommand.Create(() =>
        {
            IsEditMode = !IsEditMode;
        });

        ToggleToolboxVisibleCommand = ReactiveCommand.Create(() =>
        {
            IsToolboxVisible = !IsToolboxVisible;
        });

        NewCommand = ReactiveCommand.Create(New);

        OpenCommand = ReactiveCommand.CreateFromTask(async () => await Open());

        SaveCommand = ReactiveCommand.CreateFromTask(async () => await Save());

        ExportCommand = ReactiveCommand.CreateFromTask(async () => await Export());

        ExitCommand = ReactiveCommand.Create(() =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
        });

        AboutCommand = ReactiveCommand.Create(() =>
        {
            // TODO: Show about dialog window.
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

    public bool IsEditMode
    {
        get => _isEditMode;
        set => this.RaiseAndSetIfChanged(ref _isEditMode, value);
    }

    public bool IsToolboxVisible
    {
        get => _isToolboxVisible;
        set => this.RaiseAndSetIfChanged(ref _isToolboxVisible, value);
    }

    public ICommand ToggleEditModeCommand { get; }

    public ICommand ToggleToolboxVisibleCommand { get; }

    public ICommand NewCommand { get; }

    public ICommand OpenCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand ExportCommand { get; }

    public ICommand ExitCommand { get; }

    public ICommand AboutCommand { get; }

    private void New()
    {
        Drawing = _factory.CreateDrawing();
        Drawing.Serializer = _serializer;
    }

    private async Task Open()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
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
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
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
            
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
        {
            return;
        }

        var dlg = new SaveFileDialog() { Title = "Save" };
        dlg.Filters.Add(new FileDialogFilter() { Name = "Png", Extensions = { "png" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "Svg", Extensions = { "svg" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "Pdf", Extensions = { "pdf" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "Xps", Extensions = { "xps" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "Skp", Extensions = { "skp" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
        dlg.InitialFileName = Path.GetFileNameWithoutExtension("drawing");
        dlg.DefaultExtension = "png";

        var result = await dlg.ShowAsync(window);
        if (result is { } path)
        {
            try
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
                    using var stream = File.Create(path);
                    PngRenderer.Render(preview, size, stream);
                }

                if (path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = File.Create(path);
                    SvgRenderer.Render(preview, size, stream);
                }

                if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = File.Create(path);
                    PdfRenderer.Render(preview, size, stream, 96);
                }

                if (path.EndsWith("xps", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = File.Create(path);
                    XpsRenderer.Render(control, size, stream, 96);
                }

                if (path.EndsWith("skp", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = File.Create(path);
                    SkpRenderer.Render(control, size, stream);
                }

                preview.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    public void PrintNetList(IDrawingNode? drawing)
    {
        if (drawing?.Connectors is null || drawing.Nodes is null)
        {
            return;
        }

        foreach (var connector in drawing.Connectors)
        {
            if (connector.Start is { } start && connector.End is { } end)
            {
                Debug.WriteLine($"{start.Parent?.Name}:{start.Name} -> {end.Parent?.Name}:{end.Name}");
            }
        }
    }
}
