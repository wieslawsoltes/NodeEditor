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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class ConnectorsSelectedBehavior : Behavior<ItemsControl>
{
    private IDisposable? _dataContextDisposable;
    private IDrawingNode? _drawingNode;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { })
        {
            _dataContextDisposable = AssociatedObject
                .GetObservable(StyledElement.DataContextProperty)
                .Subscribe(new AnonymousObserver<object?>(
                    x =>
                    {
                        if (x is IDrawingNode drawingNode)
                        {
                            if (_drawingNode == drawingNode)
                            {
                                _drawingNode.SelectionChanged -= DrawingNode_SelectionChanged;
                            }

                            RemoveSelectedPseudoClasses(AssociatedObject);

                            _drawingNode = drawingNode;

                            _drawingNode.SelectionChanged += DrawingNode_SelectionChanged;
                        }
                        else
                        {
                            RemoveSelectedPseudoClasses(AssociatedObject);
                        }
                    }));
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is { })
        {
            if (_drawingNode is { })
            {
                _drawingNode.SelectionChanged -= DrawingNode_SelectionChanged;
            }

            _dataContextDisposable?.Dispose();
        }
    }

    private void DrawingNode_SelectionChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject?.DataContext is not IDrawingNode)
        {
            return;
        }

        if (_drawingNode is { })
        {
            var selectedNodes = _drawingNode.GetSelectedNodes();
            var selectedConnectors = _drawingNode.GetSelectedConnectors();

            if (selectedNodes is { Count: > 0 } || selectedConnectors is { Count: > 0 })
            {
                AddSelectedPseudoClasses(AssociatedObject);
            }
            else
            {
                RemoveSelectedPseudoClasses(AssociatedObject);
            }
        }
    }

    private void AddSelectedPseudoClasses(ItemsControl itemsControl)
    {
        foreach (var control in itemsControl.GetRealizedContainers())
        {
            if (control is not { DataContext: IConnector connector } containerControl)
            {
                continue;
            }

            var selectedConnectors = _drawingNode?.GetSelectedConnectors();

            if (_drawingNode is { } && selectedConnectors is { } && selectedConnectors.Contains(connector))
            {
                if (containerControl is ContentPresenter { Child: { } child })
                {
                    if (child.Classes is IPseudoClasses pseudoClasses)
                    {
                        pseudoClasses.Add(":selected");
                    }
                }
            }
            else
            {
                if (containerControl is ContentPresenter { Child: { } child })
                {
                    if (child.Classes is IPseudoClasses pseudoClasses)
                    {
                        pseudoClasses.Remove(":selected");
                    }
                }
            }
        }
    }

    private static void RemoveSelectedPseudoClasses(ItemsControl itemsControl)
    {
        foreach (var control in itemsControl.GetRealizedContainers())
        {
            if (control is not { DataContext: IConnector } containerControl)
            {
                continue;
            }

            if (containerControl is ContentPresenter { Child: { } child })
            {
                if (child.Classes is IPseudoClasses pseudoClasses)
                {
                    pseudoClasses.Remove(":selected");
                }
            }
        }
    }
}
