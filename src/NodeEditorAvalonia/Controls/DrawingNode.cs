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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls;

public class DrawingNode : TemplatedControl
{
    public static readonly StyledProperty<Control?> InputSourceProperty = 
        AvaloniaProperty.Register<DrawingNode, Control?>(nameof(InputSource));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty = 
        AvaloniaProperty.Register<DrawingNode, Canvas?>(nameof(AdornerCanvas));

    public static readonly StyledProperty<bool> EnableSnapProperty = 
        AvaloniaProperty.Register<DrawingNode, bool>(nameof(EnableSnap));

    public static readonly StyledProperty<double> SnapXProperty = 
        AvaloniaProperty.Register<DrawingNode, double>(nameof(SnapX), 1.0);

    public static readonly StyledProperty<double> SnapYProperty = 
        AvaloniaProperty.Register<DrawingNode, double>(nameof(SnapY), 1.0);

    public static readonly StyledProperty<bool> EnableGridProperty = 
        AvaloniaProperty.Register<DrawingNode, bool>(nameof(EnableGrid));

    public static readonly StyledProperty<double> GridCellWidthProperty = 
        AvaloniaProperty.Register<DrawingNode, double>(nameof(GridCellWidth));

    public static readonly StyledProperty<double> GridCellHeightProperty = 
        AvaloniaProperty.Register<DrawingNode, double>(nameof(GridCellHeight));

    public Control? InputSource
    {
        get => GetValue(InputSourceProperty);
        set => SetValue(InputSourceProperty, value);
    }

    public Canvas? AdornerCanvas
    {
        get => GetValue(AdornerCanvasProperty);
        set => SetValue(AdornerCanvasProperty, value);
    }

    public bool EnableSnap
    {
        get => GetValue(EnableSnapProperty);
        set => SetValue(EnableSnapProperty, value);
    }

    public double SnapX
    {
        get => GetValue(SnapXProperty);
        set => SetValue(SnapXProperty, value);
    }

    public double SnapY
    {
        get => GetValue(SnapYProperty);
        set => SetValue(SnapYProperty, value);
    }

    public bool EnableGrid
    {
        get => GetValue(EnableGridProperty);
        set => SetValue(EnableGridProperty, value);
    }

    public double GridCellWidth
    {
        get => GetValue(GridCellWidthProperty);
        set => SetValue(GridCellWidthProperty, value);
    }

    public double GridCellHeight
    {
        get => GetValue(GridCellHeightProperty);
        set => SetValue(GridCellHeightProperty, value);
    }
}
