using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeEditor.Controls;
using NodeEditor.Mvvm;
using NodeEditor.Services;
using NodeEditorDemo.Services;
using NodeEditorDemo.Views;

namespace NodeEditorDemo.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    [ObservableProperty] private EditorViewModel? _editor;
    [ObservableProperty] private bool _isToolboxVisible;
    [ObservableProperty] private bool _enableConnectionValidation = true;

    public MainViewViewModel()
    {
        _isToolboxVisible = true;
    }

    partial void OnEditorChanging(EditorViewModel? value)
    {
        if (Editor is not null)
        {
            Editor.PropertyChanged -= OnEditorPropertyChanged;
        }
    }

    partial void OnEditorChanged(EditorViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        value.PropertyChanged += OnEditorPropertyChanged;
        SyncConnectionValidation();
    }

    private void OnEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.Drawing))
        {
            SyncConnectionValidation();
        }
    }

    partial void OnEnableConnectionValidationChanged(bool value)
    {
        ApplyConnectionValidation();
    }

    private void SyncConnectionValidation()
    {
        if (Editor?.Drawing?.Settings is null)
        {
            return;
        }

        EnableConnectionValidation = Editor.Drawing.Settings.ConnectionValidator is not null;
    }

    private void ApplyConnectionValidation()
    {
        if (Editor?.Drawing?.Settings is null)
        {
            return;
        }

        Editor.Drawing.Settings.ConnectionValidator = EnableConnectionValidation
            ? BaseConnectionValidation.TypeCompatibility
            : null;
    }

    [RelayCommand]
    private void ToggleToolboxVisible()
    {
        IsToolboxVisible = !IsToolboxVisible;
    }

    [RelayCommand]
    private void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    [RelayCommand]
    private void About()
    {
        // TODO: About dialog.
    }

    [RelayCommand]
    private void New()
    {
        if (Editor?.Factory is not null)
        {
            Editor.Drawing = Editor.Factory.CreateDrawing();
            Editor.Drawing.SetSerializer(Editor.Serializer);
        }
    }

    private List<FilePickerFileType> GetOpenFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetSaveFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetExportFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.ImagePng,
            StorageService.ImageSvg,
            StorageService.Pdf,
            StorageService.Xps,
            StorageService.ImageSkp,
            StorageService.All
        };
    }

    private List<MacroDefinition> BuildMacros()
    {
        var macros = new List<MacroDefinition>
        {
            new("file.new", "New Drawing", NewCommand, category: "File"),
            new("file.open", "Open Drawing", OpenCommand, category: "File"),
            new("file.save", "Save Drawing", SaveCommand, category: "File"),
            new("file.export", "Export Drawing", ExportCommand, category: "File"),
            new("view.toolbox", "Toggle Toolbox", ToggleToolboxVisibleCommand, category: "View")
        };

        if (Editor?.Drawing is { } drawing)
        {
            macros.Add(new MacroDefinition("draw.ink.toggle", "Toggle Ink Mode", drawing.DrawInkCommand, category: "Draw"));
            macros.Add(new MacroDefinition("draw.ink.addpen", "Add Pen", drawing.AddPenCommand, category: "Draw"));
            macros.Add(new MacroDefinition("draw.ink.convert", "Convert Ink", drawing.ConvertInkCommand, category: "Draw"));
            macros.Add(new MacroDefinition("draw.ink.clear", "Clear Ink", drawing.ClearInkCommand, category: "Draw"));
            macros.Add(new MacroDefinition("view.grid.toggle", "Toggle Grid", new RelayCommand(() =>
            {
                drawing.Settings.EnableGrid = !drawing.Settings.EnableGrid;
            }), category: "View"));
            macros.Add(new MacroDefinition("view.snap.toggle", "Toggle Snap", new RelayCommand(() =>
            {
                drawing.Settings.EnableSnap = !drawing.Settings.EnableSnap;
            }), category: "View"));
        }

        return macros;
    }

    [RelayCommand]
    private void ShowMacros()
    {
        var macros = BuildMacros();
        if (macros.Count == 0)
        {
            return;
        }

        var viewModel = new MacroPickerViewModel(macros);
        var window = new MacroPickerWindow
        {
            DataContext = viewModel
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime
            && lifetime.MainWindow is Window owner)
        {
            window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }
    }

    [RelayCommand]
    private async Task Open()
    {
        if (Editor?.Serializer is null)
        {
            return;
        }

        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open drawing",
            FileTypeFilter = GetOpenFileTypes(),
            AllowMultiple = false
        });

        var file = result.FirstOrDefault();

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var drawing = Editor.Serializer.Deserialize<DrawingNodeViewModel?>(json);
                if (drawing is not null)
                {
                    Editor.Drawing = drawing;
                    Editor.Drawing.SetSerializer(Editor.Serializer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Editor?.Serializer is null)
        {
            return;
        }

        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save drawing",
            FileTypeChoices = GetSaveFileTypes(),
            SuggestedFileName = Path.GetFileNameWithoutExtension("drawing"),
            DefaultExtension = "json",
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            try
            {
                var json = Editor.Serializer.Serialize(Editor.Drawing);
                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    [RelayCommand]
    public async Task Export()
    {
        if (Editor?.Drawing is null)
        {
            return;
        }

        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export drawing",
            FileTypeChoices = GetExportFileTypes(),
            SuggestedFileName = Path.GetFileNameWithoutExtension("drawing"),
            DefaultExtension = "png",
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            try
            {
                var control = new DrawingNode
                {
                    DrawingSource = Editor.Drawing,
                    Width = Editor.Drawing.Width,
                    Height = Editor.Drawing.Height,
                };

                var root = new ExportRoot
                {
                    Width = Editor.Drawing.Width,
                    Height = Editor.Drawing.Height,
                    Child = control
                };

                root.ApplyTemplate();
                root.InvalidateMeasure();
                root.InvalidateArrange();
                root.UpdateLayout();

                var size = new Size(Editor.Drawing.Width, Editor.Drawing.Height);

                if (file.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    using var ms = new MemoryStream();
                    ExportRenderer.RenderPng(root, size, ms);
                    await using var stream = await file.OpenWriteAsync();
                    ms.Position = 0;
                    await stream.WriteAsync(ms.ToArray());
                }

                if (file.Name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    using var ms = new MemoryStream();
                    ExportRenderer.RenderSvg(root, size, ms);
                    await using var stream = await file.OpenWriteAsync();
                    ms.Position = 0;
                    await stream.WriteAsync(ms.ToArray());
                }

                if (file.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    using var ms = new MemoryStream();
                    ExportRenderer.RenderPdf(root, size, ms, 96);
                    await using var stream = await file.OpenWriteAsync();
                    ms.Position = 0;
                    await stream.WriteAsync(ms.ToArray());
                }

                if (file.Name.EndsWith("xps", StringComparison.OrdinalIgnoreCase))
                {
                    using var ms = new MemoryStream();
                    ExportRenderer.RenderXps(control, size, ms, 96);
                    await using var stream = await file.OpenWriteAsync();
                    ms.Position = 0;
                    await stream.WriteAsync(ms.ToArray());
                }

                if (file.Name.EndsWith("skp", StringComparison.OrdinalIgnoreCase))
                {
                    using var ms = new MemoryStream();
                    ExportRenderer.RenderSkp(control, size, ms);
                    await using var stream = await file.OpenWriteAsync();
                    ms.Position = 0;
                    await stream.WriteAsync(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
