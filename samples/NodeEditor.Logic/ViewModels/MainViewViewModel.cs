using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeEditor.Controls;
using NodeEditor.Mvvm;
using NodeEditor.Services;
using NodeEditorLogic.Models;
using NodeEditorLogic.Services;
using NodeEditorLogic.Views;
using NodeEditorLogic.ViewModels.Nodes;
using NodeEditorLogic.ViewModels.Waveforms;

namespace NodeEditorLogic.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    private readonly DispatcherTimer _simulationTimer;
    private readonly LogicSimulationService _simulation;
    private readonly Dictionary<string, LogicWaveformSignal> _waveformLookup = new();
    private DrawingNodeViewModel? _attachedDrawing;

    [ObservableProperty] private EditorViewModel? _editor;
    [ObservableProperty] private bool _isToolboxVisible = true;
    [ObservableProperty] private string _toolboxFilter = string.Empty;
    [ObservableProperty] private ObservableCollection<ToolboxCategoryViewModel> _toolboxCategories = new();
    [ObservableProperty] private ObservableCollection<ToolboxCategoryViewModel> _filteredToolboxCategories = new();
    [ObservableProperty] private LogicEditorTool _activeTool = LogicEditorTool.Wire;
    [ObservableProperty] private bool _enableConnectionValidation = true;
    [ObservableProperty] private NodeViewModel? _selectedNode;
    [ObservableProperty] private bool _autoEvaluate = true;
    [ObservableProperty] private bool _isSimulationRunning;
    [ObservableProperty] private int _simulationSpeed = 4;
    [ObservableProperty] private int _tickCount;
    [ObservableProperty] private string _simulationStatus = "Idle";
    [ObservableProperty] private ObservableCollection<string> _simulationMessages = new();
    [ObservableProperty] private ObservableCollection<LogicWaveformSignal> _waveformSignals = new();
    [ObservableProperty] private int _waveformDepth = 32;
    [ObservableProperty] private bool _trackInputs;
    [ObservableProperty] private bool _trackOutputs = true;
    [ObservableProperty] private bool _trackClocks = true;

    public MainViewViewModel()
        : this(new LogicSimulationService(new LogicComponentLibrary()))
    {
    }

    public MainViewViewModel(LogicSimulationService simulation)
    {
        _simulation = simulation;
        _simulationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _simulationTimer.Tick += (_, _) => StepSimulation();
    }

    public string RunButtonLabel => IsSimulationRunning ? "Stop" : "Run";

    public bool IsSelectToolActive
    {
        get => ActiveTool == LogicEditorTool.Select;
        set
        {
            if (value && ActiveTool != LogicEditorTool.Select)
            {
                ActiveTool = LogicEditorTool.Select;
            }
        }
    }

    public bool IsWireToolActive
    {
        get => ActiveTool == LogicEditorTool.Wire;
        set
        {
            if (value && ActiveTool != LogicEditorTool.Wire)
            {
                ActiveTool = LogicEditorTool.Wire;
            }
        }
    }

    public bool IsInkToolActive
    {
        get => ActiveTool == LogicEditorTool.Ink;
        set
        {
            if (value && ActiveTool != LogicEditorTool.Ink)
            {
                ActiveTool = LogicEditorTool.Ink;
            }
        }
    }

    partial void OnIsSimulationRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(RunButtonLabel));
    }

    partial void OnActiveToolChanged(LogicEditorTool value)
    {
        OnPropertyChanged(nameof(IsSelectToolActive));
        OnPropertyChanged(nameof(IsWireToolActive));
        OnPropertyChanged(nameof(IsInkToolActive));
        ApplyToolMode();
    }

    partial void OnSimulationSpeedChanged(int value)
    {
        var clamped = Math.Clamp(value, 1, 12);
        if (clamped != value)
        {
            SimulationSpeed = clamped;
            return;
        }

        var interval = 1000.0 / clamped;
        _simulationTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(40, interval));
    }

    partial void OnWaveformDepthChanged(int value)
    {
        var clamped = Math.Clamp(value, 8, 128);
        if (clamped != value)
        {
            WaveformDepth = clamped;
            return;
        }

        foreach (var signal in WaveformSignals)
        {
            while (signal.Samples.Count > WaveformDepth)
            {
                signal.Samples.RemoveAt(0);
            }
        }
    }

    partial void OnToolboxFilterChanged(string value)
    {
        UpdateToolboxFilter();
    }

    partial void OnAutoEvaluateChanged(bool value)
    {
        if (value && !IsSimulationRunning)
        {
            EvaluateSimulation();
        }
    }

    partial void OnEnableConnectionValidationChanged(bool value)
    {
        ApplyConnectionValidation();
    }

    partial void OnEditorChanging(EditorViewModel? value)
    {
        if (Editor is not null)
        {
            Editor.PropertyChanged -= OnEditorPropertyChanged;
        }

        AttachDrawing(null);
    }

    partial void OnEditorChanged(EditorViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        value.PropertyChanged += OnEditorPropertyChanged;
        AttachDrawing(value.Drawing as DrawingNodeViewModel);
        ApplyToolMode();
    }

    private void OnEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.Drawing))
        {
            AttachDrawing(Editor?.Drawing as DrawingNodeViewModel);
        }
    }

    public void SetToolboxCategories(ObservableCollection<ToolboxCategoryViewModel> categories)
    {
        ToolboxCategories = categories;
        UpdateToolboxFilter();
    }

    private void UpdateToolboxFilter()
    {
        FilteredToolboxCategories.Clear();

        var filter = ToolboxFilter.Trim();
        if (string.IsNullOrWhiteSpace(filter))
        {
            foreach (var category in ToolboxCategories)
            {
                FilteredToolboxCategories.Add(category);
            }

            return;
        }

        foreach (var category in ToolboxCategories)
        {
            var matches = new ObservableCollection<NodeEditor.Model.INodeTemplate>();
            foreach (var template in category.Templates)
            {
                if (template.Title is not null && template.Title.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(template);
                }
            }

            if (matches.Count > 0)
            {
                FilteredToolboxCategories.Add(new ToolboxCategoryViewModel
                {
                    Title = category.Title,
                    Templates = matches,
                    IsExpanded = true
                });
            }
        }
    }

    private void ApplyToolMode()
    {
        if (Editor?.Drawing?.Settings is null)
        {
            return;
        }

        Editor.Drawing.Settings.EnableConnections = ActiveTool == LogicEditorTool.Wire;
        Editor.Drawing.Settings.EnableInk = true;
        Editor.Drawing.Settings.IsInkMode = ActiveTool == LogicEditorTool.Ink;

        if (ActiveTool != LogicEditorTool.Wire && Editor.Drawing.IsConnectorMoving())
        {
            Editor.Drawing.CancelConnector();
        }
    }

    private void AttachDrawing(DrawingNodeViewModel? drawing)
    {
        if (_attachedDrawing is not null)
        {
            _attachedDrawing.SelectionChanged -= OnDrawingSelectionChanged;
            DetachNodeEvents(_attachedDrawing);
        }

        _attachedDrawing = drawing;

        if (_attachedDrawing is null)
        {
            SelectedNode = null;
            return;
        }

        _attachedDrawing.SelectionChanged += OnDrawingSelectionChanged;
        AttachNodeEvents(_attachedDrawing);
        SelectedNode = _attachedDrawing.GetSelectedNodes()?.FirstOrDefault() as NodeViewModel;
        ApplyToolMode();
        EnableConnectionValidation = _attachedDrawing.Settings.ConnectionValidator is not null;

        if (AutoEvaluate)
        {
            EvaluateSimulation();
        }
    }

    private void ApplyConnectionValidation()
    {
        if (Editor?.Drawing?.Settings is null)
        {
            return;
        }

        Editor.Drawing.Settings.ConnectionValidator = EnableConnectionValidation
            ? LogicConnectionValidation.TypeCompatibility
            : null;
    }

    private void AttachNodeEvents(DrawingNodeViewModel drawing)
    {
        if (drawing.Nodes is INotifyCollectionChanged notifyNodes)
        {
            notifyNodes.CollectionChanged += OnNodesChanged;
        }

        if (drawing.Connectors is INotifyCollectionChanged notifyConnectors)
        {
            notifyConnectors.CollectionChanged += OnConnectorsChanged;
        }

        if (drawing.Nodes is not null)
        {
            foreach (var node in drawing.Nodes.OfType<NodeViewModel>())
            {
                HookNode(node);
            }
        }
    }

    private void DetachNodeEvents(DrawingNodeViewModel drawing)
    {
        if (drawing.Nodes is INotifyCollectionChanged notifyNodes)
        {
            notifyNodes.CollectionChanged -= OnNodesChanged;
        }

        if (drawing.Connectors is INotifyCollectionChanged notifyConnectors)
        {
            notifyConnectors.CollectionChanged -= OnConnectorsChanged;
        }

        if (drawing.Nodes is not null)
        {
            foreach (var node in drawing.Nodes.OfType<NodeViewModel>())
            {
                UnhookNode(node);
            }
        }
    }

    private void OnNodesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is NodeViewModel node)
                {
                    UnhookNode(node);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is NodeViewModel node)
                {
                    HookNode(node);
                }
            }
        }

        if (AutoEvaluate)
        {
            EvaluateSimulation();
        }
    }

    private void OnConnectorsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (AutoEvaluate)
        {
            EvaluateSimulation();
        }
    }

    private void HookNode(NodeViewModel node)
    {
        if (node.Content is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged += OnNodeContentChanged;
        }

        if (node.Content is LogicNodeContentViewModel logicContent)
        {
            logicContent.HostNode = node;
            if (logicContent.InputPins.Count == 0 && logicContent.OutputPins.Count == 0 && node.Pins is not null)
            {
                foreach (var pin in node.Pins.OfType<LogicPinViewModel>())
                {
                    if (pin.Kind == LogicPinKind.Input)
                    {
                        logicContent.InputPins.Add(pin);
                    }
                    else
                    {
                        logicContent.OutputPins.Add(pin);
                    }
                }
            }

            switch (logicContent)
            {
                case LogicBusInputNodeViewModel busInput:
                    LogicNodeFactory.RefreshBusInputPins(node, busInput);
                    break;
                case LogicBusOutputNodeViewModel busOutput:
                    LogicNodeFactory.RefreshBusOutputPins(node, busOutput);
                    break;
                case LogicBusSplitNodeViewModel busSplit:
                    LogicNodeFactory.RefreshBusSplitPins(node, busSplit);
                    break;
                case LogicBusMergeNodeViewModel busMerge:
                    LogicNodeFactory.RefreshBusMergePins(node, busMerge);
                    break;
                default:
                    LogicNodeFactory.RefreshNodeLayout(node, logicContent);
                    break;
            }
        }
    }

    private void UnhookNode(NodeViewModel node)
    {
        if (node.Content is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnNodeContentChanged;
        }
    }

    private void OnNodeContentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!AutoEvaluate || IsSimulationRunning)
        {
            return;
        }

        if (e.PropertyName is nameof(LogicInputNodeViewModel.IsOn)
            or nameof(LogicClockNodeViewModel.IsRunning)
            or nameof(LogicClockNodeViewModel.Period)
            or nameof(LogicClockNodeViewModel.HighTicks)
            or nameof(LogicNodeContentViewModel.ComponentId)
            or nameof(LogicNodeContentViewModel.PropagationDelay)
            or nameof(LogicBusInputNodeViewModel.BusValue))
        {
            EvaluateSimulation();
        }

        if (e.PropertyName is nameof(LogicBusInputNodeViewModel.BusWidth)
            or nameof(LogicBusOutputNodeViewModel.BusWidth)
            or nameof(LogicBusSplitNodeViewModel.BitCount)
            or nameof(LogicBusMergeNodeViewModel.BitCount))
        {
            if (sender is LogicNodeContentViewModel busContent && busContent.HostNode is { } host)
            {
                switch (busContent)
                {
                    case LogicBusInputNodeViewModel busInput:
                        LogicNodeFactory.RefreshBusInputPins(host, busInput);
                        break;
                    case LogicBusOutputNodeViewModel busOutput:
                        LogicNodeFactory.RefreshBusOutputPins(host, busOutput);
                        break;
                    case LogicBusSplitNodeViewModel busSplit:
                        LogicNodeFactory.RefreshBusSplitPins(host, busSplit);
                        break;
                    case LogicBusMergeNodeViewModel busMerge:
                        LogicNodeFactory.RefreshBusMergePins(host, busMerge);
                        break;
                }
            }

            if (AutoEvaluate)
            {
                EvaluateSimulation();
            }
        }
    }

    private void OnDrawingSelectionChanged(object? sender, EventArgs e)
    {
        SelectedNode = _attachedDrawing?.GetSelectedNodes()?.FirstOrDefault() as NodeViewModel;
    }

    private void EvaluateSimulation()
    {
        if (Editor?.Drawing is null)
        {
            return;
        }

        var result = _simulation.Evaluate(Editor.Drawing);
        UpdateSimulationStatus(result);
    }

    private void StepSimulation()
    {
        if (Editor?.Drawing is null)
        {
            return;
        }

        var result = _simulation.Step(Editor.Drawing);
        TickCount = _simulation.TickCount;
        UpdateSimulationStatus(result);
        RecordWaveforms();
    }

    private void UpdateSimulationStatus(LogicSimulationResult result)
    {
        SimulationMessages.Clear();
        foreach (var message in result.Messages)
        {
            SimulationMessages.Add(message);
        }

        SimulationStatus = result.IsStable
            ? $"Stable in {result.Iterations} iterations"
            : "Unstable: see diagnostics";
    }

    private void RecordWaveforms()
    {
        if (Editor?.Drawing?.Nodes is null)
        {
            return;
        }

        if (!TrackInputs && !TrackOutputs && !TrackClocks)
        {
            return;
        }

        foreach (var node in Editor.Drawing.Nodes.OfType<NodeViewModel>())
        {
            switch (node.Content)
            {
                case LogicInputNodeViewModel input when TrackInputs:
                {
                    var label = ResolveLabel(input.Label, input.Title, "Input");
                    AddWaveformSample($"in:{label}", label, new[] { input.IsOn ? LogicValue.High : LogicValue.Low });
                    break;
                }
                case LogicBusInputNodeViewModel busInput when TrackInputs:
                {
                    var label = ResolveLabel(busInput.Label, busInput.Title, "Bus In");
                    AddWaveformSample($"bus-in:{label}", label, LogicSignalHelper.CreateBusFromInt(busInput.BusValue, busInput.BusWidth));
                    break;
                }
                case LogicOutputNodeViewModel output when TrackOutputs:
                {
                    var label = ResolveLabel(output.Label, output.Title, "Output");
                    AddWaveformSample($"out:{label}", label, new[] { output.ObservedValue });
                    break;
                }
                case LogicBusOutputNodeViewModel busOutput when TrackOutputs:
                {
                    var label = ResolveLabel(busOutput.Label, busOutput.Title, "Bus Out");
                    if (busOutput.InputPins.Count > 0)
                    {
                        AddWaveformSample($"bus-out:{label}", label, busOutput.InputPins[0].BusValue);
                    }
                    break;
                }
                case LogicClockNodeViewModel clock when TrackClocks:
                {
                    var label = ResolveLabel(clock.Title, null, "Clock");
                    AddWaveformSample($"clk:{label}", label, new[] { clock.State });
                    break;
                }
            }
        }
    }

    private static string ResolveLabel(string? primary, string? secondary, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(primary))
        {
            return primary;
        }

        if (!string.IsNullOrWhiteSpace(secondary))
        {
            return secondary;
        }

        return fallback;
    }

    private void AddWaveformSample(string key, string name, LogicValue[] signal)
    {
        var signalKey = key ?? name;
        if (!_waveformLookup.TryGetValue(signalKey, out var waveform))
        {
            var isBus = signal.Length > 1;
            waveform = new LogicWaveformSignal(signalKey, name, isBus, signal.Length);
            _waveformLookup[signalKey] = waveform;
            WaveformSignals.Add(waveform);
        }

        var display = signal.Length > 1
            ? LogicSignalHelper.ToHexString(signal)
            : signal[0] switch
            {
                LogicValue.High => "1",
                LogicValue.Low => "0",
                _ => "X"
            };

        var aggregate = signal.Length > 1 ? LogicSignalHelper.Aggregate(signal) : signal[0];
        waveform.AddSample(new LogicWaveformSample(TickCount, display, aggregate), WaveformDepth);
    }

    private void StopTimer()
    {
        _simulationTimer.Stop();
        IsSimulationRunning = false;
    }

    private void StartTimer()
    {
        if (Editor?.Drawing is null)
        {
            return;
        }

        IsSimulationRunning = true;
        _simulationTimer.Start();
    }

    [RelayCommand]
    private void ToggleToolboxVisible()
    {
        IsToolboxVisible = !IsToolboxVisible;
    }

    [RelayCommand]
    private void ToggleRun()
    {
        if (IsSimulationRunning)
        {
            StopTimer();
        }
        else
        {
            StartTimer();
        }
    }

    [RelayCommand]
    private void Step()
    {
        StopTimer();
        StepSimulation();
    }

    [RelayCommand]
    private void Reset()
    {
        StopTimer();

        if (Editor?.Drawing is null)
        {
            return;
        }

        _simulation.Reset(Editor.Drawing);
        TickCount = _simulation.TickCount;
        ClearWaveforms();
        EvaluateSimulation();
    }

    [RelayCommand]
    private void ClearWaveforms()
    {
        _waveformLookup.Clear();
        WaveformSignals.Clear();
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
            TickCount = 0;
            ClearWaveforms();
            EvaluateSimulation();
        }
    }

    private static ObservableCollection<FilePickerFileType> GetOpenFileTypes()
    {
        return new ObservableCollection<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private static ObservableCollection<FilePickerFileType> GetSaveFileTypes()
    {
        return new ObservableCollection<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private static ObservableCollection<FilePickerFileType> GetExportFileTypes()
    {
        return new ObservableCollection<FilePickerFileType>
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
            new("simulation.toggle", "Run/Stop Simulation", ToggleRunCommand, category: "Simulation"),
            new("simulation.step", "Step Simulation", StepCommand, category: "Simulation"),
            new("simulation.reset", "Reset Simulation", ResetCommand, category: "Simulation"),
            new("simulation.clear", "Clear Waveforms", ClearWaveformsCommand, category: "Simulation"),
            new("file.new", "New Circuit", NewCommand, category: "File"),
            new("file.open", "Open Circuit", OpenCommand, category: "File"),
            new("file.save", "Save Circuit", SaveCommand, category: "File"),
            new("file.export", "Export Circuit", ExportCommand, category: "File"),
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
            Title = "Open circuit",
            FileTypeFilter = GetOpenFileTypes(),
            AllowMultiple = false
        });

        var file = result.Count > 0 ? result[0] : null;

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
                    drawing.SetFactory(LogicDrawingNodeFactory.Instance);
                    Editor.Drawing = drawing;
                    Editor.Drawing.SetSerializer(Editor.Serializer);
                    TickCount = 0;
                    EvaluateSimulation();
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
            Title = "Save circuit",
            FileTypeChoices = GetSaveFileTypes(),
            SuggestedFileName = Path.GetFileNameWithoutExtension("circuit"),
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
            Title = "Export circuit",
            FileTypeChoices = GetExportFileTypes(),
            SuggestedFileName = Path.GetFileNameWithoutExtension("circuit"),
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
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
