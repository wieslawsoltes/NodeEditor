using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class ConnectorsSelectedBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<ConnectorsSelectedBehavior, IDrawingNode?>(nameof(DrawingSource));

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    private IDisposable? _dataContextDisposable;
    private IDrawingNode? _drawingNode;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
        {
            return;
        }

        _dataContextDisposable = this
            .GetObservable(DrawingSourceProperty)
            .Subscribe(new AnonymousObserver<IDrawingNode?>(
                drawingNode =>
                {
                    if (_drawingNode is not null)
                    {
                        _drawingNode.SelectionChanged -= DrawingNode_SelectionChanged;
                    }

                    RemoveSelectedPseudoClasses(AssociatedObject);

                    _drawingNode = drawingNode;

                    if (_drawingNode is not null)
                    {
                        _drawingNode.SelectionChanged += DrawingNode_SelectionChanged;
                    }
                }));
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is null)
        {
            return;
        }

        if (_drawingNode is not null)
        {
            _drawingNode.SelectionChanged -= DrawingNode_SelectionChanged;
        }

        _dataContextDisposable?.Dispose();
    }

    private void DrawingNode_SelectionChanged(object? sender, EventArgs e)
    {
        if (DrawingSource is not IDrawingNode)
        {
            return;
        }

        if (_drawingNode is null)
        {
            return;
        }

        if (AssociatedObject is null)
        {
            return;
        }

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

    private void AddSelectedPseudoClasses(ItemsControl itemsControl)
    {
        foreach (var control in itemsControl.GetRealizedContainers())
        {
            if (control is not { DataContext: IConnector connector } containerControl)
            {
                continue;
            }

            var selectedConnectors = _drawingNode?.GetSelectedConnectors();

            var connectorControl = FindConnectorControl(containerControl as ContentPresenter);
            if (connectorControl is null)
            {
                continue;
            }

            if (_drawingNode is not null && selectedConnectors is not null && selectedConnectors.Contains(connector))
            {
                if (connectorControl.Classes is IPseudoClasses pseudoClasses)
                {
                    pseudoClasses.Add(":selected");
                }
            }
            else
            {
                if (connectorControl.Classes is IPseudoClasses pseudoClasses)
                {
                    pseudoClasses.Remove(":selected");
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

            var connectorControl = FindConnectorControl(containerControl as ContentPresenter);
            if (connectorControl?.Classes is IPseudoClasses pseudoClasses)
            {
                pseudoClasses.Remove(":selected");
            }
        }
    }

    private static Connector? FindConnectorControl(ContentPresenter? presenter)
    {
        if (presenter?.Child is Connector connector)
        {
            return connector;
        }

        if (presenter?.Child is not Visual visual)
        {
            return null;
        }

        foreach (var descendant in visual.GetVisualDescendants())
        {
            if (descendant is Connector found)
            {
                return found;
            }
        }

        return null;
    }
}
