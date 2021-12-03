using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class NodesSelectedBehavior : Behavior<ItemsControl>
{
    private IDisposable? _isEditModeDisposable;
    private IDisposable? _dataContextDisposable;
    private INotifyPropertyChanged? _drawingNodePropertyChanged;
    private IDrawingNode? _drawingNode;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { })
        {
            _isEditModeDisposable = AssociatedObject.GetObservable(DrawingNode.IsEditModeProperty)
                .Subscribe(x =>
                {
                    if (x == false)
                    {
                        RemoveSelectedPseudoClasses(AssociatedObject);
                    }
                });

            _dataContextDisposable = AssociatedObject
                .GetObservable(StyledElement.DataContextProperty)
                .Subscribe(x =>
                {
                    if (x is IDrawingNode drawingNode)
                    {
                        if (_drawingNode == drawingNode)
                        {
                            if (_drawingNodePropertyChanged != null)
                            {
                                _drawingNodePropertyChanged.PropertyChanged -= DrawingNode_PropertyChanged;
                            }
                        }

                        RemoveSelectedPseudoClasses(AssociatedObject);

                        _drawingNode = drawingNode;

                        if (_drawingNode is INotifyPropertyChanged notifyPropertyChanged)
                        {
                            _drawingNodePropertyChanged = notifyPropertyChanged;
                            _drawingNodePropertyChanged.PropertyChanged += DrawingNode_PropertyChanged;
                        }
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
            if (_drawingNodePropertyChanged is { })
            {
                _drawingNodePropertyChanged.PropertyChanged -= DrawingNode_PropertyChanged;
            }

            _isEditModeDisposable?.Dispose();
            _dataContextDisposable?.Dispose();
        }
    }

    private void DrawingNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (AssociatedObject?.DataContext is not IDrawingNode)
        {
            return;
        }

        if (e.PropertyName == nameof(IDrawingNode.SelectedNodes) || e.PropertyName == nameof(IDrawingNode.SelectedConnectors))
        {
            if (_drawingNode is { })
            {
                if (_drawingNode.SelectedNodes is { Count: > 0 } || _drawingNode.SelectedConnectors is { Count: > 0 })
                {
                    AddSelectedPseudoClasses(AssociatedObject);
                }
                else
                {
                    RemoveSelectedPseudoClasses(AssociatedObject);
                }
            }
        }
    }

    private void AddSelectedPseudoClasses(ItemsControl itemsControl)
    {
        foreach (var container in itemsControl.ItemContainerGenerator.Containers)
        {
            if (container.ContainerControl is not { DataContext: INode node } containerControl)
            {
                continue;
            }

            if (_drawingNode is { } && _drawingNode.SelectedNodes is { } && _drawingNode.SelectedNodes.Contains(node))
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
        foreach (var container in itemsControl.ItemContainerGenerator.Containers)
        {
            if (container.ContainerControl is not { DataContext: INode } containerControl)
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