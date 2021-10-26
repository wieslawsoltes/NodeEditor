using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using NodeEditor.ViewModels;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private DrawingNodeViewModel? _drawing;

        public MainWindowViewModel()
        {
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

        public DrawingNodeViewModel? Drawing
        {
            get => _drawing;
            set => this.RaiseAndSetIfChanged(ref _drawing, value);
        }

        public ICommand OpenCommand { get; }

        public ICommand SaveCommand { get; }
    }
}
