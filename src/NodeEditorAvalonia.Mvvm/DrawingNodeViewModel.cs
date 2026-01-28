using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

public partial class DrawingNodeSettingsViewModel : ObservableObject, IDrawingNodeSettings
{
    [ObservableProperty] private bool _enableConnections = true;
    [ObservableProperty] private bool _requireDirectionalConnections;
    [ObservableProperty] private bool _requireMatchingBusWidth;
    [ObservableProperty] private bool _enableMultiplePinConnections;
    [ObservableProperty] private bool _allowSelfConnections = true;
    [ObservableProperty] private bool _allowDuplicateConnections = true;
    [ObservableProperty] private ConnectionValidationHandler? _connectionValidator;
    [ObservableProperty] private bool _enableInk = true;
    [ObservableProperty] private bool _isInkMode;
    [ObservableProperty] private IList<InkPen>? _inkPens = new ObservableCollection<InkPen>();
    [ObservableProperty] private InkPen? _activePen;
    [ObservableProperty] private bool _enableSnap;
    [ObservableProperty] private double _snapX;
    [ObservableProperty] private double _snapY;
    [ObservableProperty] private double _nudgeStep = 1.0;
    [ObservableProperty] private double _nudgeMultiplier = 10.0;
    [ObservableProperty] private bool _enableGrid;
    [ObservableProperty] private double _gridCellWidth;
    [ObservableProperty] private double _gridCellHeight;
    [ObservableProperty] private bool _enableGuides = true;
    [ObservableProperty] private double _guideSnapTolerance = 6.0;
    [ObservableProperty] private bool _enableConnectorRouting = true;
    [ObservableProperty] private double _routingGridSize = 10.0;
    [ObservableProperty] private double _routingObstaclePadding = 8.0;
    [ObservableProperty] private ConnectorRoutingAlgorithm _routingAlgorithm = ConnectorRoutingAlgorithm.Auto;
    [ObservableProperty] private double _routingBendPenalty = 0.6;
    [ObservableProperty] private double _routingDiagonalCost = 1.4;
    [ObservableProperty] private double _routingCornerRadius = 10.0;
    [ObservableProperty] private int _routingMaxCells = 200;
    [ObservableProperty] private ConnectorStyle _defaultConnectorStyle = ConnectorStyle.Bezier;

    public DrawingNodeSettingsViewModel()
    {
        if (_inkPens is { Count: > 0 })
        {
            _activePen = _inkPens[0];
            return;
        }

        var pen = new InkPen
        {
            Name = "Pen 1",
            Color = 0xFF1E1E1E,
            Thickness = 2.0,
            Opacity = 1.0
        };

        _inkPens = new ObservableCollection<InkPen> { pen };
        _activePen = pen;
    }
}

public partial class DrawingNodeViewModel : NodeViewModel, IDrawingNode, IUndoRedoHost
{
    private sealed class DrawingSnapshot
    {
        public IList<INode>? Nodes { get; set; }
        public IList<IConnector>? Connectors { get; set; }
        public IList<InkStroke>? InkStrokes { get; set; }
    }

    private const int MaxUndoEntries = 100;
    private DrawingNodeEditor _editor;
    private ISet<INode>? _selectedNodes;
    private ISet<IConnector>? _selectedConnectors;
    private INodeSerializer? _serializer;
    private readonly List<string> _undoStack = new();
    private readonly List<string> _redoStack = new();
    private int _undoBatchDepth;
    private string? _pendingSnapshot;
    private bool _isRestoring;
    private readonly RelayCommand _undoCommand;
    private readonly RelayCommand _redoCommand;
    [ObservableProperty] private IList<INode>? _nodes;
    [ObservableProperty] private IList<IConnector>? _connectors;
    [ObservableProperty] private IList<InkStroke>? _inkStrokes = new ObservableCollection<InkStroke>();
    [ObservableProperty] private IDrawingNodeSettings _settings;

    private static readonly uint[] InkPalette =
    {
        0xFF1E1E1E,
        0xFFE53935,
        0xFF1E88E5,
        0xFF43A047,
        0xFFFDD835,
        0xFF8E24AA,
        0xFFFB8C00
    };

    public DrawingNodeViewModel()
        : this(null)
    {
    }

    public DrawingNodeViewModel(IDrawingNodeFactory? factory)
    {
        _editor = new DrawingNodeEditor(this, factory ?? DrawingNodeFactory.Instance);

        _settings = new DrawingNodeSettingsViewModel();

        CutNodesCommand = new RelayCommand(CutNodes);

        CopyNodesCommand = new RelayCommand(CopyNodes);

        PasteNodesCommand = new RelayCommand(PasteNodes);

        DuplicateNodesCommand = new RelayCommand(DuplicateNodes);

        DrawInkCommand = new RelayCommand(ToggleInkMode);

        ConvertInkCommand = new RelayCommand(ConvertInkToNodes);

        AddPenCommand = new RelayCommand(AddPen);

        ClearInkCommand = new RelayCommand(ClearInk);

        _undoCommand = new RelayCommand(Undo, () => CanUndo);
        _redoCommand = new RelayCommand(Redo, () => CanRedo);
        UndoCommand = _undoCommand;
        RedoCommand = _redoCommand;

        SelectAllNodesCommand = new RelayCommand(SelectAllNodes);

        DeselectAllNodesCommand = new RelayCommand(DeselectAllNodes);

        DeleteNodesCommand = new RelayCommand(DeleteNodes);

        AlignNodesCommand = new RelayCommand<NodeAlignment>(AlignSelectedNodes);

        DistributeNodesCommand = new RelayCommand<NodeDistribution>(DistributeSelectedNodes);

        OrderNodesCommand = new RelayCommand<NodeOrder>(OrderSelectedNodes);

        LockSelectionCommand = new RelayCommand(LockSelection);

        UnlockSelectionCommand = new RelayCommand(UnlockSelection);

        HideSelectionCommand = new RelayCommand(HideSelection);

        ShowSelectionCommand = new RelayCommand(ShowSelection);

        ShowAllCommand = new RelayCommand(ShowAll);
    }
 
    public event SelectionChangedEventHandler? SelectionChanged;
    public event EventHandler<ConnectionRejectedEventArgs>? ConnectionRejected;

    public ICommand CutNodesCommand { get; }

    public ICommand CopyNodesCommand { get; }

    public ICommand PasteNodesCommand { get; }

    public ICommand DuplicateNodesCommand { get; }

    public ICommand DrawInkCommand { get; }

    public ICommand ConvertInkCommand { get; }

    public ICommand AddPenCommand { get; }

    public ICommand ClearInkCommand { get; }

    public ICommand UndoCommand { get; }

    public ICommand RedoCommand { get; }

    public ICommand SelectAllNodesCommand { get; }

    public ICommand DeselectAllNodesCommand { get; }

    public ICommand DeleteNodesCommand { get; }

    public ICommand AlignNodesCommand { get; }

    public ICommand DistributeNodesCommand { get; }

    public ICommand OrderNodesCommand { get; }

    public ICommand LockSelectionCommand { get; }

    public ICommand UnlockSelectionCommand { get; }

    public ICommand HideSelectionCommand { get; }

    public ICommand ShowSelectionCommand { get; }

    public ICommand ShowAllCommand { get; }

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    public void NotifySelectionChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);

    public void NotifyConnectionRejected(IPin start, IPin end)
    {
        ConnectionRejected?.Invoke(this, new ConnectionRejectedEventArgs(start, end));
    }

    public void NotifyDeselectedNodes()
    {
        var selectedNodes = GetSelectedNodes();
        if (selectedNodes is not null)
        {
            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.OnDeselected();
            }
        }
    }

    public void NotifyDeselectedConnectors()
    {
        var selectedConnectors = GetSelectedConnectors();
        if (selectedConnectors is not null)
        {
            foreach (var selectedConnector in selectedConnectors)
            {
                selectedConnector.OnDeselected();
            }
        }
    }

    public ISet<INode>? GetSelectedNodes() => _selectedNodes;

    public void SetSelectedNodes(ISet<INode>? nodes) => _selectedNodes = nodes;

    public ISet<IConnector>? GetSelectedConnectors() => _selectedConnectors;

    public void SetSelectedConnectors(ISet<IConnector>? connectors) => _selectedConnectors = connectors;

    public INodeSerializer? GetSerializer() => _serializer;

    public void SetSerializer(INodeSerializer? serializer)
    {
        _serializer = serializer;
        _undoStack.Clear();
        _redoStack.Clear();
        _undoBatchDepth = 0;
        _pendingSnapshot = null;
        UpdateUndoState();
    }

    public T? Clone<T>(T source) => _editor.Clone(source);

    public bool IsPinConnected(IPin pin) => _editor.IsPinConnected(pin);

    public bool IsConnectorMoving() => _editor.IsConnectorMoving();

    public void CancelConnector() => _editor.CancelConnector();

    public virtual bool CanSelectNodes() => _editor.CanSelectNodes();

    public virtual bool CanSelectConnectors() => _editor.CanSelectConnectors();

    public virtual bool CanConnectPin(IPin pin) => _editor.CanConnectPin(pin);

    public virtual void DrawingLeftPressed(double x, double y) => _editor.DrawingLeftPressed(x, y);

    public virtual void DrawingRightPressed(double x, double y) => _editor.DrawingRightPressed(x, y);

    public virtual void ConnectorLeftPressed(IPin pin, bool showWhenMoving) => _editor.ConnectorLeftPressed(pin, showWhenMoving);

    public virtual void ConnectorMove(double x, double y) => _editor.ConnectorMove(x, y);

    public virtual void CutNodes() => ExecuteWithUndo(_editor.CutNodes);

    public virtual void CopyNodes() => _editor.CopyNodes();

    public virtual void PasteNodes() => ExecuteWithUndo(_editor.PasteNodes);

    public virtual void DuplicateNodes() => ExecuteWithUndo(_editor.DuplicateNodes);

    public virtual void ToggleInkMode()
    {
        if (Settings is null)
        {
            return;
        }

        Settings.EnableInk = true;
        Settings.IsInkMode = !Settings.IsInkMode;
    }

    public virtual void ClearInk() => ExecuteWithUndo(ClearInkCore);

    public virtual void AddPen()
    {
        if (Settings is null)
        {
            return;
        }

        var pens = Settings.InkPens;
        if (pens is null)
        {
            pens = new ObservableCollection<InkPen>();
            Settings.InkPens = pens;
        }

        var index = pens.Count;
        var color = InkPalette[index % InkPalette.Length];
        var pen = new InkPen
        {
            Name = $"Pen {index + 1}",
            Color = color,
            Thickness = 2.0,
            Opacity = 1.0
        };

        pens.Add(pen);
        Settings.ActivePen = pen;
        Settings.EnableInk = true;
        Settings.IsInkMode = true;
    }

    public virtual void ConvertInkToNodes() => ExecuteWithUndo(ConvertInkToNodesCore);

    public virtual void DeleteNodes() => ExecuteWithUndo(_editor.DeleteNodes);

    public virtual void AlignSelectedNodes(NodeAlignment alignment) =>
        ExecuteWithUndo(() => _editor.AlignSelectedNodes(alignment));

    public virtual void DistributeSelectedNodes(NodeDistribution distribution) =>
        ExecuteWithUndo(() => _editor.DistributeSelectedNodes(distribution));

    public virtual void OrderSelectedNodes(NodeOrder order) => ExecuteWithUndo(() => _editor.OrderSelectedNodes(order));

    public virtual void LockSelection() => ExecuteWithUndo(_editor.LockSelection);

    public virtual void UnlockSelection() => ExecuteWithUndo(_editor.UnlockSelection);

    public virtual void HideSelection() => ExecuteWithUndo(_editor.HideSelection);

    public virtual void ShowSelection() => ExecuteWithUndo(_editor.ShowSelection);

    public virtual void ShowAll() => ExecuteWithUndo(_editor.ShowAll);

    public virtual void SelectAllNodes() => _editor.SelectAllNodes();

    public virtual void DeselectAllNodes() => _editor.DeselectAllNodes();

    public void SetFactory(IDrawingNodeFactory factory)
    {
        _editor = new DrawingNodeEditor(this, factory ?? DrawingNodeFactory.Instance);
    }

    public void Undo()
    {
        if (_serializer is null || _undoStack.Count == 0)
        {
            return;
        }

        var undoIndex = _undoStack.Count - 1;
        var snapshot = _undoStack[undoIndex];
        _undoStack.RemoveAt(undoIndex);

        var current = CaptureSnapshot();
        if (current is not null)
        {
            PushHistory(_redoStack, current);
        }

        ApplySnapshot(snapshot);
        UpdateUndoState();
    }

    public void Redo()
    {
        if (_serializer is null || _redoStack.Count == 0)
        {
            return;
        }

        var redoIndex = _redoStack.Count - 1;
        var snapshot = _redoStack[redoIndex];
        _redoStack.RemoveAt(redoIndex);

        var current = CaptureSnapshot();
        if (current is not null)
        {
            PushHistory(_undoStack, current);
        }

        ApplySnapshot(snapshot);
        UpdateUndoState();
    }

    public void BeginUndoBatch()
    {
        if (_serializer is null || _isRestoring)
        {
            return;
        }

        if (_undoBatchDepth == 0)
        {
            _pendingSnapshot = CaptureSnapshot();
        }

        _undoBatchDepth++;
    }

    public void EndUndoBatch()
    {
        if (_serializer is null || _isRestoring || _undoBatchDepth == 0)
        {
            return;
        }

        _undoBatchDepth--;
        if (_undoBatchDepth > 0)
        {
            return;
        }

        var before = _pendingSnapshot;
        _pendingSnapshot = null;

        if (before is null)
        {
            return;
        }

        var after = CaptureSnapshot();
        if (after is null || string.Equals(before, after, StringComparison.Ordinal))
        {
            return;
        }

        PushHistory(_undoStack, before);
        _redoStack.Clear();
        UpdateUndoState();
    }

    private void ExecuteWithUndo(Action action)
    {
        BeginUndoBatch();
        try
        {
            action();
        }
        finally
        {
            EndUndoBatch();
        }
    }

    private static void PushHistory(List<string> stack, string snapshot)
    {
        stack.Add(snapshot);
        if (stack.Count > MaxUndoEntries)
        {
            stack.RemoveAt(0);
        }
    }

    private string? CaptureSnapshot()
    {
        if (_serializer is null)
        {
            return null;
        }

        var snapshot = new DrawingSnapshot
        {
            Nodes = Nodes,
            Connectors = Connectors,
            InkStrokes = InkStrokes
        };

        return _serializer.Serialize(snapshot);
    }

    private void ApplySnapshot(string snapshot)
    {
        if (_serializer is null)
        {
            return;
        }

        var state = _serializer.Deserialize<DrawingSnapshot?>(snapshot);
        if (state is null)
        {
            return;
        }

        _isRestoring = true;
        try
        {
            NotifyDeselectedNodes();
            NotifyDeselectedConnectors();
            SetSelectedNodes(null);
            SetSelectedConnectors(null);

            Nodes = state.Nodes;
            Connectors = state.Connectors;
            InkStrokes = state.InkStrokes;

            if (Nodes is { Count: > 0 })
            {
                foreach (var node in Nodes)
                {
                    node.Parent = this;
                    if (node.Pins is null)
                    {
                        continue;
                    }

                    foreach (var pin in node.Pins)
                    {
                        pin.Parent = node;
                    }
                }
            }

            if (Connectors is { Count: > 0 })
            {
                foreach (var connector in Connectors)
                {
                    connector.Parent = this;
                }
            }

            NotifySelectionChanged();
        }
        finally
        {
            _isRestoring = false;
        }
    }

    private void UpdateUndoState()
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        _undoCommand.NotifyCanExecuteChanged();
        _redoCommand.NotifyCanExecuteChanged();
    }

    private void ClearInkCore()
    {
        InkStrokes?.Clear();
    }

    private void ConvertInkToNodesCore()
    {
        if (InkStrokes is null || InkStrokes.Count == 0)
        {
            return;
        }

        var nodes = Nodes;
        if (nodes is null)
        {
            nodes = new ObservableCollection<INode>();
            Nodes = nodes;
        }

        var strokes = InkStrokes.ToList();
        foreach (var stroke in strokes)
        {
            if (stroke.Points is null || stroke.Points.Count < 2)
            {
                continue;
            }

            var minX = stroke.Points.Min(point => point.X);
            var minY = stroke.Points.Min(point => point.Y);
            var maxX = stroke.Points.Max(point => point.X);
            var maxY = stroke.Points.Max(point => point.Y);
            var padding = Math.Max(2.0, stroke.Thickness);
            var left = minX - padding;
            var top = minY - padding;
            var width = Math.Max(1.0, maxX - minX + padding * 2.0);
            var height = Math.Max(1.0, maxY - minY + padding * 2.0);

            var normalized = new InkStroke
            {
                Color = stroke.Color,
                Thickness = stroke.Thickness,
                Opacity = stroke.Opacity,
                Name = stroke.Name,
                Points = stroke.Points
                    .Select(point => new InkPoint(point.X - left, point.Y - top, point.Pressure, point.Timestamp))
                    .ToList()
            };

            var shape = new InkShape { Stroke = normalized };

            var node = new NodeViewModel
            {
                Name = "Ink",
                Parent = this,
                X = left,
                Y = top,
                Width = width,
                Height = height,
                Content = shape,
                IsVisible = true
            };

            nodes.Add(node);
            node.OnCreated();
        }

        InkStrokes.Clear();
    }
}
