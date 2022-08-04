using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeEditor.Model;

namespace NodeEditor.ViewModels;

public partial class DrawingNodeViewModel : NodeViewModel, IDrawingNode
{
    private readonly DrawingNodeEditor _editor;
    private ISet<INode>? _selectedNodes;
    private ISet<IConnector>? _selectedConnectors;
    private INodeSerializer? _serializer;
    [ObservableProperty] private IList<INode>? _nodes;
    [ObservableProperty] private IList<IConnector>? _connectors;
    [ObservableProperty] private bool _enableMultiplePinConnections;

    public DrawingNodeViewModel()
    {
        _editor = new DrawingNodeEditor(this, DrawingNodeFactory.Instance);

        CutCommand = new RelayCommand(CutNodes);

        CopyCommand = new RelayCommand(CopyNodes);

        PasteCommand = new RelayCommand(PasteNodes);

        DuplicateCommand = new RelayCommand(DuplicateNodes);

        SelectAllCommand = new RelayCommand(SelectAllNodes);

        DeselectAllCommand = new RelayCommand(DeselectAllNodes);

        DeleteCommand = new RelayCommand(DeleteNodes);

    }
 
    public event SelectionChangedEventHandler? SelectionChanged;

    public ICommand CutCommand { get; }

    public ICommand CopyCommand { get; }

    public ICommand PasteCommand { get; }

    public ICommand DuplicateCommand { get; }

    public ICommand SelectAllCommand { get; }

    public ICommand DeselectAllCommand { get; }

    public ICommand DeleteCommand { get; }

    public void NotifySelectionChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);

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
