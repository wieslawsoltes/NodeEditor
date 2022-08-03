using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels;

[DataContract(IsReference = true)]
public class DrawingNodeViewModel : NodeViewModel, IDrawingNode
{
    private IList<INode>? _nodes;
    private IList<IConnector>? _connectors;
    private ISet<INode>? _selectedNodes;
    private ISet<IConnector>? _selectedConnectors;
    private INodeSerializer? _serializer;
    private bool _enableMultiplePinConnections;
    private readonly DrawingNodeEditor _editor;

    public DrawingNodeViewModel()
    {
        _editor = new DrawingNodeEditor(this, DrawingNodeFactory.Instance);

        CutCommand = ReactiveCommand.Create(CutNodes);

        CopyCommand = ReactiveCommand.Create(CopyNodes);

        PasteCommand = ReactiveCommand.Create(PasteNodes);

        DuplicateCommand = ReactiveCommand.Create(DuplicateNodes);

        SelectAllCommand = ReactiveCommand.Create(SelectAllNodes);

        DeselectAllCommand = ReactiveCommand.Create(DeselectAllNodes);

        DeleteCommand = ReactiveCommand.Create(DeleteNodes);

    }

    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public IList<INode>? Nodes
    {
        get => _nodes;
        set => this.RaiseAndSetIfChanged(ref _nodes, value);
    }

    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public IList<IConnector>? Connectors
    {
        get => _connectors;
        set => this.RaiseAndSetIfChanged(ref _connectors, value);
    }

    [IgnoreDataMember]
    public ISet<INode>? SelectedNodes
    {
        get => _selectedNodes;
        set => this.RaiseAndSetIfChanged(ref _selectedNodes, value);
    }

    [IgnoreDataMember]
    public ISet<IConnector>? SelectedConnectors
    {
        get => _selectedConnectors;
        set => this.RaiseAndSetIfChanged(ref _selectedConnectors, value);
    }

    [IgnoreDataMember]
    public INodeSerializer? Serializer
    {
        get => _serializer;
        set => this.RaiseAndSetIfChanged(ref _serializer, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public bool EnableMultiplePinConnections
    {
        get => _enableMultiplePinConnections;
        set => this.RaiseAndSetIfChanged(ref _enableMultiplePinConnections, value);
    }

    public ICommand CutCommand { get; }

    public ICommand CopyCommand { get; }

    public ICommand PasteCommand { get; }

    public ICommand DuplicateCommand { get; }

    public ICommand SelectAllCommand { get; }

    public ICommand DeselectAllCommand { get; }

    public ICommand DeleteCommand { get; }

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
