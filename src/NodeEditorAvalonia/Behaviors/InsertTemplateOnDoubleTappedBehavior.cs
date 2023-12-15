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
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class InsertTemplateOnDoubleTappedBehavior : Behavior<ListBoxItem>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingProperty = 
        AvaloniaProperty.Register<InsertTemplateOnDoubleTappedBehavior, IDrawingNode?>(nameof(Drawing));

    public IDrawingNode? Drawing
    {
        get => GetValue(DrawingProperty);
        set => SetValue(DrawingProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is { })
        {
            AssociatedObject.DoubleTapped += DoubleTapped; 
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is { })
        {
            AssociatedObject.DoubleTapped -= DoubleTapped; 
        }
    }

    private void DoubleTapped(object? sender, RoutedEventArgs args)
    {
        if (AssociatedObject is { DataContext: INodeTemplate template } && Drawing is { } drawing)
        {
            var node = drawing.Clone(template.Template);
            if (node is { })
            {
                node.Parent = drawing;
                node.Move(0.0, 0.0);
                drawing.Nodes?.Add(node);
                node.OnCreated();
            }
        }
    }
}
