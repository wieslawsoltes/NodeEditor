﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace NodeEditor.Behaviors;

public abstract class DefaultDropHandler : AvaloniaObject, IDropHandler
{
    public static Point GetPosition(Control? relativeTo, DragEventArgs e)
    {
        relativeTo ??= e.Source as Control;
        var point = relativeTo is not null ? e.GetPosition(relativeTo) : new Point();
        return point;
    }

    public static Point GetPositionScreen(object? sender, DragEventArgs e)
    {
        var relativeTo = e.Source as Control;
        var point = relativeTo is not null ? e.GetPosition(relativeTo) : new Point();
        var visual = relativeTo as Visual;
        if (visual is null)
        {
            return new Point();
        }
        var screenPoint = visual.PointToScreen(point).ToPoint(1.0);
        return screenPoint;
    }

    public virtual void Enter(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        if (Validate(sender, e, sourceContext, targetContext, null) == false)
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
        }
        else
        {
            e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
            e.Handled = true;
        }
    }

    public virtual void Over(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        if (Validate(sender, e, sourceContext, targetContext, null) == false)
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
        }
        else
        {
            e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
            e.Handled = true;
        }
    }

    public virtual void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        if (Execute(sender, e, sourceContext, targetContext, null) == false)
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
        }
        else
        {
            e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
            e.Handled = true;
        }
    }

    public virtual void Leave(object? sender, RoutedEventArgs e)
    {
        Cancel(sender, e);
    }

    public virtual bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        return false;
    }

    public virtual bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        return false;
    }

    public virtual void Cancel(object? sender, RoutedEventArgs e)
    {
    }
}
