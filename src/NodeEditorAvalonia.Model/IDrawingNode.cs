using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NodeEditor.Model;

public delegate void SelectionChangedEventHandler(object? sender, EventArgs e);

public interface IDrawingNode : INode
{
    public event SelectionChangedEventHandler? SelectionChanged;
    IList<INode>? Nodes { get; set; }
    IList<IConnector>? Connectors { get; set; }
    ISet<INode>? GetSelectedNodes();
    bool EnableMultiplePinConnections { get; set; }
    bool EnableSnap { get; set; }
    double SnapX { get; set; }
    double SnapY { get; set; }
    bool EnableGrid { get; set; }
    double GridCellWidth { get; set; }
    double GridCellHeight { get; set; }
    ICommand CutNodesCommand { get; }
    ICommand CopyNodesCommand { get; }
    ICommand PasteNodesCommand { get; }
    ICommand DuplicateNodesCommand { get; }
    ICommand SelectAllNodesCommand { get; }
    ICommand DeselectAllNodesCommand { get; }
    ICommand DeleteNodesCommand { get; }
    void NotifySelectionChanged();
    void NotifyDeselectedNodes();
    void NotifyDeselectedConnectors();
    void  SetSelectedNodes(ISet<INode>? nodes);
    ISet<IConnector>? GetSelectedConnectors();
    void  SetSelectedConnectors(ISet<IConnector>? connectors);
    INodeSerializer? GetSerializer();
    void SetSerializer(INodeSerializer? serializer);
    public T? Clone<T>(T source);
    bool IsPinConnected(IPin pin);
    bool IsConnectorMoving();
    void CancelConnector();
    bool CanSelectNodes();
    bool CanSelectConnectors();
    bool CanConnectPin(IPin pin);
    void DrawingLeftPressed(double x, double y);
    void DrawingRightPressed(double x, double y);
    void ConnectorLeftPressed(IPin pin, bool showWhenMoving);
    void ConnectorMove(double x, double y);
    void CutNodes();
    void CopyNodes();
    void PasteNodes();
    void DuplicateNodes();
    void DeleteNodes();
    void SelectAllNodes();
    void DeselectAllNodes();
}
