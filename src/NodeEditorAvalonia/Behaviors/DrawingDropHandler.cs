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
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingDropHandler : DefaultDropHandler
{
    public static readonly StyledProperty<Control?> RelativeToProperty =
        AvaloniaProperty.Register<DrawingDropHandler, Control?>(nameof(RelativeTo));

    public Control? RelativeTo
    {
        get => GetValue(RelativeToProperty);
        set => SetValue(RelativeToProperty, value);
    }

    private bool Validate(IDrawingNode drawing, object? sender, DragEventArgs e, bool bExecute)
    {
        var relativeTo = RelativeTo ?? sender as Control;
        if (relativeTo is null)
        {
            return false;
        }
        var point = GetPosition(relativeTo, e);

        if (relativeTo is DrawingNode drawingNode)
        {
            point = SnapHelper.Snap(point, drawingNode.SnapX, drawingNode.SnapY, drawingNode.EnableSnap);
        }

        if (e.Data.Contains(DataFormats.Text))
        {
            var text = e.Data.GetText();

            if (bExecute)
            {
                if (text is { })
                {
                    // TODO: text
                }
            }

            return true;
        }

        foreach (var format in e.Data.GetDataFormats())
        {
            var data = e.Data.Get(format);

            switch (data)
            {
                case INodeTemplate template:
                {
                    if (bExecute)
                    {
                        var node = drawing.Clone(template.Template);
                        if (node is { })
                        {
                            node.Parent = drawing;
                            node.Move(point.X, point.Y);
                            drawing.Nodes?.Add(node);
                            node.OnCreated();
                        }
                    }
                    return true;
                }
            }
        }

        if (e.Data.Contains(DataFormats.Files))
        {
            // ReSharper disable once UnusedVariable
            var files = e.Data.GetFiles()?.ToArray();
            if (bExecute)
            {
                // TODO: files, point.X, point.Y
            }

            return true;
        }

        return false;
    }

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (targetContext is IDrawingNode drawing)
        {
            return Validate(drawing, sender, e, false);
        }

        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (targetContext is IDrawingNode drawing)
        {
            return Validate(drawing, sender, e, true);
        }

        return false;
    }
}
