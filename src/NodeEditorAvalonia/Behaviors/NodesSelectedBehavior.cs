using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class NodesSelectedBehavior : Behavior<ItemsControl>
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
                .Subscribe(x =>
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
                });
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
            if (control is not { DataContext: INode node } containerControl)
            {
                continue;
            }

            var selectedNodes = _drawingNode?.GetSelectedNodes();

            if (_drawingNode is { } && selectedNodes is { } && selectedNodes.Contains(node))
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
            if (control is not { DataContext: INode } containerControl)
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
