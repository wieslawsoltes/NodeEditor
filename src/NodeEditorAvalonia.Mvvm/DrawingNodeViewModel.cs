using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

public partial class DrawingNodeViewModel : NodeViewModel, IDrawingNode
{
    private readonly DrawingNodeEditor _editor;
    private ISet<INode>? _selectedNodes;
    private ISet<IConnector>? _selectedConnectors;
    private INodeSerializer? _serializer;
    [ObservableProperty] private IList<INode>? _nodes;
    [ObservableProperty] private IList<IConnector>? _connectors;
    [ObservableProperty] private bool _enableMultiplePinConnections;
    [ObservableProperty] private bool _enableSnap;
    [ObservableProperty] private double _snapX;
    [ObservableProperty] private double _snapY;
    [ObservableProperty] private bool _enableGrid;
    [ObservableProperty] private double _gridCellWidth;
    [ObservableProperty] private double _gridCellHeight;

    public DrawingNodeViewModel()
    {
        _editor = new DrawingNodeEditor(this, DrawingNodeFactory.Instance);

        CutNodesCommand = new RelayCommand(CutNodes);

        CopyNodesCommand = new RelayCommand(CopyNodes);

        PasteNodesCommand = new RelayCommand(PasteNodes);

        DuplicateNodesCommand = new RelayCommand(DuplicateNodes);

        SelectAllNodesCommand = new RelayCommand(SelectAllNodes);

        DeselectAllNodesCommand = new RelayCommand(DeselectAllNodes);

        DeleteNodesCommand = new RelayCommand(DeleteNodes);
    }
 
    public event SelectionChangedEventHandler? SelectionChanged;

    public ICommand CutNodesCommand { get; }

    public ICommand CopyNodesCommand { get; }

    public ICommand PasteNodesCommand { get; }

    public ICommand DuplicateNodesCommand { get; }

    public ICommand SelectAllNodesCommand { get; }

    public ICommand DeselectAllNodesCommand { get; }

    public ICommand DeleteNodesCommand { get; }

    public void NotifySelectionChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);

    public void NotifyDeselectedNodes()
    {
        var selectedNodes = GetSelectedNodes();
        if (selectedNodes is { })
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
        if (selectedConnectors is { })
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

    public void SetSerializer(INodeSerializer? serializer) => _serializer = serializer;

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

    public virtual void CutNodes() => _editor.CutNodes();

    public virtual void CopyNodes() => _editor.CopyNodes();

    public virtual void PasteNodes() => _editor.PasteNodes();

    public virtual void DuplicateNodes() => _editor.DuplicateNodes();

    public virtual void DeleteNodes() => _editor.DeleteNodes();

    public virtual void SelectAllNodes() => _editor.SelectAllNodes();

    public virtual void DeselectAllNodes() => _editor.DeselectAllNodes();
}
