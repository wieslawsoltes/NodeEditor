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
using System.Collections.ObjectModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

public static class NodeViewModelExtensions
{
    public static IPin AddPin(this NodeViewModel node, double x, double y, double width, double height, PinAlignment alignment = PinAlignment.None, string? name = null)
    {
        var pin = new PinViewModel
        {
            Name = name,
            Parent = node,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Alignment = alignment
        };

        node.Pins ??= new ObservableCollection<IPin>();
        node.Pins.Add(pin);

        return pin;
    }
}
