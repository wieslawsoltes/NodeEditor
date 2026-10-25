using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorDemo.Services;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels;

/// <summary>Framework-independent presentation model for the interactive browser sample.</summary>
public sealed class ZipTieDemoViewModel : ReactiveObject, IDisposable
{
    private readonly INodeSerializer _serializer;
    private DrawingNodeViewModel _drawing;
    private IZipTie? _observedTie;
    private string? _savedDrawing;
    private string _status = "Drag either collar to shape the bundle. Shift-drag a collar to move the whole harness.";
    private bool _disposed;

    public ZipTieDemoViewModel()
        : this(new NodeSerializer(typeof(ObservableCollection<>)))
    {
    }

    public ZipTieDemoViewModel(INodeSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _drawing = ZipTieDemo.CreateDrawing();
        _drawing.SetSerializer(serializer);
        ObserveDrawing(_drawing);

        ResetCommand = ReactiveCommand.Create(Reset);
        SelectWiresCommand = ReactiveCommand.Create(SelectWires);
        SaveSnapshotCommand = ReactiveCommand.Create(SaveSnapshot);
        LoadSnapshotCommand = ReactiveCommand.Create(LoadSnapshot,
            this.WhenAnyValue(x => x.HasSnapshot));
        SetWireStyleCommand = ReactiveCommand.Create<ConnectorStyle>(SetWireStyle);
        AddBendCommand = ReactiveCommand.Create(AddBend,
            this.WhenAnyValue(x => x.CanEditSelectedTie));
    }

    public DrawingNodeViewModel Drawing
    {
        get => _drawing;
        private set
        {
            if (ReferenceEquals(_drawing, value))
            {
                return;
            }

            _drawing.PropertyChanged -= OnDrawingChanged;
            _drawing.SelectionChanged -= OnSelectionChanged;
            if (_observedTie is not null)
            {
                _observedTie.PropertyChanged -= OnTieChanged;
                _observedTie = null;
            }

            this.RaiseAndSetIfChanged(ref _drawing, value);
            ObserveDrawing(value);
            NotifySelectionProperties();
        }
    }

    public string Status
    {
        get => _status;
        private set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public bool HasSnapshot => _savedDrawing is not null;

    public bool CanEditSelectedTie => Drawing.SelectedZipTie is { IsLocked: false, IsVisible: true };

    public string SelectionText => Drawing.SelectedZipTie is { } tie
        ? $"{tie.Name ?? "Zip tie"} · {tie.Connectors.Count} wires"
        : $"{Drawing.GetSelectedConnectors()?.Count ?? 0} wires selected";

    public double Spacing
    {
        get => Drawing.SelectedZipTie?.Spacing ?? 7;
        set
        {
            if (!CanEditSelectedTie || Drawing.SelectedZipTie is not { } tie || !double.IsFinite(value))
            {
                return;
            }

            var spacing = Math.Clamp(value, 2, 32);
            if (Math.Abs(tie.Spacing - spacing) < 0.001)
            {
                return;
            }

            Drawing.BeginUndoBatch();
            try
            {
                tie.Spacing = spacing;
            }
            finally
            {
                Drawing.EndUndoBatch();
            }
            this.RaisePropertyChanged();
        }
    }

    public ReactiveCommand<Unit, Unit> ResetCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectWiresCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveSnapshotCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadSnapshotCommand { get; }
    public ReactiveCommand<ConnectorStyle, Unit> SetWireStyleCommand { get; }
    public ReactiveCommand<Unit, Unit> AddBendCommand { get; }

    private void ObserveDrawing(DrawingNodeViewModel drawing)
    {
        drawing.PropertyChanged += OnDrawingChanged;
        drawing.SelectionChanged += OnSelectionChanged;
        ObserveSelectedTie();
    }

    private void OnDrawingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(DrawingNodeViewModel.SelectedZipTie))
        {
            ObserveSelectedTie();
            NotifySelectionProperties();
        }
    }

    private void ObserveSelectedTie()
    {
        if (_observedTie is not null)
        {
            _observedTie.PropertyChanged -= OnTieChanged;
        }

        _observedTie = Drawing.SelectedZipTie;
        if (_observedTie is not null)
        {
            _observedTie.PropertyChanged += OnTieChanged;
        }
    }

    private void OnSelectionChanged(object? sender, EventArgs e) => NotifySelectionProperties();

    private void OnTieChanged(object? sender, PropertyChangedEventArgs e) => NotifySelectionProperties();

    private void NotifySelectionProperties()
    {
        this.RaisePropertyChanged(nameof(Spacing));
        this.RaisePropertyChanged(nameof(SelectionText));
        this.RaisePropertyChanged(nameof(CanEditSelectedTie));
    }

    private void Reset()
    {
        var drawing = ZipTieDemo.CreateDrawing();
        drawing.SetSerializer(_serializer);
        Drawing = drawing;
        Status = "Demo reset. Drag a collar, move a socket panel, or dissolve the bundle and create a new one.";
    }

    private void SelectWires()
    {
        Drawing.NotifyDeselectedNodes();
        Drawing.SetSelectedNodes(null);
        Drawing.NotifyDeselectedConnectors();
        var selected = new HashSet<IConnector>();
        if (Drawing.Connectors is { } connectors)
        {
            foreach (var connector in connectors)
            {
                if (connector.CanSelect())
                {
                    selected.Add(connector);
                    connector.OnSelected();
                }
            }
        }

        Drawing.SetSelectedConnectors(selected);
        Drawing.NotifySelectionChanged();
        Status = "All visible wires selected. Create a tie, or attach/detach these wires from the selected bundle.";
    }

    private void SetWireStyle(ConnectorStyle style)
    {
        Drawing.BeginUndoBatch();
        try
        {
            if (Drawing.Connectors is { } connectors)
            {
                foreach (var connector in connectors)
                {
                    if (!connector.IsLocked)
                    {
                        connector.Style = style;
                    }
                }
            }
        }
        finally
        {
            Drawing.EndUndoBatch();
        }

        Status = $"Wire style: {style}. Zip ties preserve each wire's endpoints, direction and identity.";
    }

    private void AddBend()
    {
        if (!CanEditSelectedTie || Drawing.SelectedZipTie is not { } tie)
        {
            return;
        }

        Drawing.BeginUndoBatch();
        try
        {
            var previous = tie.Waypoints.Count > 0 ? tie.Waypoints[tie.Waypoints.Count - 1] : tie.Start;
            tie.Waypoints.Add(new ConnectorPoint((previous.X + tie.End.X) / 2,
                (previous.Y + tie.End.Y) / 2 - 70));
        }
        finally
        {
            Drawing.EndUndoBatch();
        }

        Status = "Added an intermediate tie. Drag its collar to bend the shared route; Straighten removes intermediate ties.";
    }

    private void SaveSnapshot()
    {
        try
        {
            _savedDrawing = _serializer.Serialize(Drawing);
            this.RaisePropertyChanged(nameof(HasSnapshot));
            Status = $"Saved a JSON snapshot ({_savedDrawing.Length:N0} characters). Edit the drawing, then Restore snapshot to verify persistence.";
        }
        catch (Exception exception)
        {
            Status = $"Snapshot could not be saved: {exception.Message}";
        }
    }

    private void LoadSnapshot()
    {
        if (_savedDrawing is null)
        {
            return;
        }

        try
        {
            var drawing = _serializer.Deserialize<DrawingNodeViewModel>(_savedDrawing);
            if (drawing is null)
            {
                Status = "Snapshot did not contain a drawing.";
                return;
            }

            drawing.SetSerializer(_serializer);
            Drawing = drawing;
            Status = $"Restored {Drawing.Connectors?.Count ?? 0} wires and {Drawing.ZipTies.Count} bundles from JSON.";
        }
        catch (Exception exception)
        {
            Status = $"Snapshot could not be restored: {exception.Message}";
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _drawing.PropertyChanged -= OnDrawingChanged;
        _drawing.SelectionChanged -= OnSelectionChanged;
        if (_observedTie is not null)
        {
            _observedTie.PropertyChanged -= OnTieChanged;
        }
        ResetCommand.Dispose();
        SelectWiresCommand.Dispose();
        SaveSnapshotCommand.Dispose();
        LoadSnapshotCommand.Dispose();
        SetWireStyleCommand.Dispose();
        AddBendCommand.Dispose();
    }
}
