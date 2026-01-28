using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NodeEditor.Model;

public delegate void SelectionChangedEventHandler(object? sender, EventArgs e);

public sealed class ConnectionRejectedEventArgs : EventArgs
{
    public IPin Start { get; }

    public IPin End { get; }

    public ConnectionRejectedEventArgs(IPin start, IPin end)
    {
        Start = start;
        End = end;
    }
}

public interface IDrawingNodeSettings
{
    bool EnableConnections { get; set; }
    bool RequireDirectionalConnections { get; set; }
    bool RequireMatchingBusWidth { get; set; }
    bool EnableMultiplePinConnections { get; set; }
    bool AllowSelfConnections { get; set; }
    bool AllowDuplicateConnections { get; set; }
    ConnectionValidationHandler? ConnectionValidator { get; set; }
    bool EnableInk { get; set; }
    bool IsInkMode { get; set; }
    IList<InkPen>? InkPens { get; set; }
    InkPen? ActivePen { get; set; }
    bool EnableSnap { get; set; }
    double SnapX { get; set; }
    double SnapY { get; set; }
    double NudgeStep { get; set; }
    double NudgeMultiplier { get; set; }
    bool EnableGrid { get; set; }
    double GridCellWidth { get; set; }
    double GridCellHeight { get; set; }
    bool EnableGuides { get; set; }
    double GuideSnapTolerance { get; set; }
    bool EnableConnectorRouting { get; set; }
    double RoutingGridSize { get; set; }
    double RoutingObstaclePadding { get; set; }
    ConnectorRoutingAlgorithm RoutingAlgorithm { get; set; }
    double RoutingBendPenalty { get; set; }
    double RoutingDiagonalCost { get; set; }
    double RoutingCornerRadius { get; set; }
    int RoutingMaxCells { get; set; }
    ConnectorStyle DefaultConnectorStyle { get; set; }
}

public interface IDrawingNode : INode
{
    public event SelectionChangedEventHandler? SelectionChanged;
    public event EventHandler<ConnectionRejectedEventArgs>? ConnectionRejected;
    IList<INode>? Nodes { get; set; }
    IList<IConnector>? Connectors { get; set; }
    IList<InkStroke>? InkStrokes { get; set; }
    IDrawingNodeSettings Settings { get; set; }
    ISet<INode>? GetSelectedNodes();
    ICommand CutNodesCommand { get; }
    ICommand CopyNodesCommand { get; }
    ICommand PasteNodesCommand { get; }
    ICommand DuplicateNodesCommand { get; }
    ICommand DrawInkCommand { get; }
    ICommand ConvertInkCommand { get; }
    ICommand AddPenCommand { get; }
    ICommand ClearInkCommand { get; }
    ICommand UndoCommand { get; }
    ICommand RedoCommand { get; }
    ICommand SelectAllNodesCommand { get; }
    ICommand DeselectAllNodesCommand { get; }
    ICommand DeleteNodesCommand { get; }
    ICommand AlignNodesCommand { get; }
    ICommand DistributeNodesCommand { get; }
    ICommand OrderNodesCommand { get; }
    ICommand LockSelectionCommand { get; }
    ICommand UnlockSelectionCommand { get; }
    ICommand HideSelectionCommand { get; }
    ICommand ShowSelectionCommand { get; }
    ICommand ShowAllCommand { get; }
    void NotifySelectionChanged();
    void NotifyConnectionRejected(IPin start, IPin end);
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
    void AlignSelectedNodes(NodeAlignment alignment);
    void DistributeSelectedNodes(NodeDistribution distribution);
    void OrderSelectedNodes(NodeOrder order);
    void LockSelection();
    void UnlockSelection();
    void HideSelection();
    void ShowSelection();
    void ShowAll();
    void SelectAllNodes();
    void DeselectAllNodes();
}
