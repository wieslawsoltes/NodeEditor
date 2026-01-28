using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Avalonia.Xaml.Interactions.DragAndDrop;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class ToolboxDragBehavior : Behavior<Control>
{
    public static readonly StyledProperty<double> DragThresholdProperty =
        AvaloniaProperty.Register<ToolboxDragBehavior, double>(nameof(DragThreshold), 6);

    private Point? _dragStart;
    private bool _dragging;

    public double DragThreshold
    {
        get => GetValue(DragThresholdProperty);
        set => SetValue(DragThresholdProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, true);
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, true);
        AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Tunnel, true);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, true);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
        AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _dragStart = e.GetPosition(AssociatedObject);
        e.Pointer.Capture(AssociatedObject);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragStart = null;
        _dragging = false;
        if (AssociatedObject is not null && Equals(e.Pointer.Captured, AssociatedObject))
        {
            e.Pointer.Capture(null);
        }
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _dragStart = null;
        _dragging = false;
    }

    private async void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragStart is null || _dragging || AssociatedObject is null)
        {
            return;
        }

        if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
        {
            _dragStart = null;
            return;
        }

        var current = e.GetPosition(AssociatedObject);
        var deltaX = Math.Abs(current.X - _dragStart.Value.X);
        var deltaY = Math.Abs(current.Y - _dragStart.Value.Y);
        if (deltaX < DragThreshold && deltaY < DragThreshold)
        {
            return;
        }

        if (AssociatedObject.DataContext is not INodeTemplate template)
        {
            _dragStart = null;
            return;
        }

        _dragging = true;
        _dragStart = null;

        try
        {
            var data = new DataObject();
            var dataFormat = ContextDropBehavior.DataFormat;
            if (!string.IsNullOrWhiteSpace(dataFormat))
            {
                data.Set(dataFormat, template);
            }
            data.Set("NodeTemplate", template);

        var title = template.Title;
        if (!string.IsNullOrWhiteSpace(title))
        {
            data.Set(DataFormats.Text, title!);
        }

            await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
        }
        finally
        {
            _dragging = false;
        }
    }
}
