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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorDemo.ViewModels.Nodes;

namespace NodeEditorDemo.Services;

public class NodeFactory : INodeFactory
{
    internal static INode CreateRectangle(double x, double y, double width, double height, string? label, double pinSize = 10)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = label }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static INode CreateEllipse(double x, double y, double width, double height, string? label, double pinSize = 10)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new EllipseViewModel { Label = label }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");
            
        return node;
    }

    internal static INode CreateSignal(double x, double y, double width = 180, double height = 30, string? label = null, bool? state = false, double pinSize = 10, string name = "SIGNAL")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new SignalViewModel { Label = label, State = state }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "IN");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "OUT");
  
        return node;
    }

    internal static INode CreateAndGate(double x, double y, double width = 60, double height = 60, double pinSize = 10, string name = "AND")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new AndGateViewModel { Label = "&" }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static INode CreateOrGate(double x, double y, double width = 60, double height = 60, int count = 1, double pinSize = 10, string name = "OR")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new OrGateViewModel { Label = "≥", Count = count}
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static IConnector CreateConnector(IPin? start, IPin? end)
    {
        return new ConnectorViewModel
        { 
            Start = start,
            End = end
        };
    }

    public IDrawingNode CreateDrawing(string? name = null)
    {
        var drawing = new DrawingNodeViewModel
        {
            Name = name,
            X = 0,
            Y = 0,
            Width = 900,
            Height = 600,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<IConnector>(),
            EnableMultiplePinConnections = false,
            EnableSnap = true,
            SnapX = 15.0,
            SnapY = 15.0,
            EnableGrid = true,
            GridCellWidth = 15.0,
            GridCellHeight = 15.0,
        };

        return drawing;
    }

    public IList<INodeTemplate> CreateTemplates()
    {
        return new ObservableCollection<INodeTemplate>
        {
            new NodeTemplateViewModel
            {
                Title = "Rectangle",
                Template = CreateRectangle(0, 0, 60, 60, "rect"),
                Preview = CreateRectangle(0, 0, 60, 60, "rect")
            },
            new NodeTemplateViewModel
            {
                Title = "Ellipse",
                Template = CreateEllipse(0, 0, 60, 60, "ellipse"),
                Preview = CreateEllipse(0, 0, 60, 60, "ellipse")
            },
            new NodeTemplateViewModel
            {
                Title = "Signal",
                Template = CreateSignal(0, 0, label: "signal", state: false),
                Preview = CreateSignal(0, 0, label: "signal", state: false)
            },
            new NodeTemplateViewModel
            {
                Title = "AND Gate",
                Template = CreateAndGate(0, 0, 60, 60),
                Preview = CreateAndGate(0, 0, 60, 60)
            },
            new NodeTemplateViewModel
            {
                Title = "OR Gate",
                Template = CreateOrGate(0, 0, 60, 60),
                Preview = CreateOrGate(0, 0, 60, 60)
            }
        };
    }
}
