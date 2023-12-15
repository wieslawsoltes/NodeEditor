/*
 * NodeEditor A node editor control for Avalonia.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
