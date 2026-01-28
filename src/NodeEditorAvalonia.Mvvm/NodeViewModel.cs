using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

[ObservableObject]
public partial class NodeViewModel : INode
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private INode? _parent;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    [ObservableProperty] private double _rotation;
    [ObservableProperty] private object? _content;
    [ObservableProperty] private IList<IPin>? _pins;
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isLocked;

    public event EventHandler<NodeCreatedEventArgs>? Created;

    public event EventHandler<NodeRemovedEventArgs>? Removed;

    public event EventHandler<NodeMovedEventArgs>? Moved;

    public event EventHandler<NodeSelectedEventArgs>? Selected;

    public event EventHandler<NodeDeselectedEventArgs>? Deselected;

    public event EventHandler<NodeResizedEventArgs>? Resized;

    public virtual bool CanSelect()
    {
        return IsVisible;
    }

    public virtual bool CanRemove()
    {
        return IsVisible && !IsLocked;
    }

    public virtual bool CanMove()
    {
        return IsVisible && !IsLocked;
    }

    public virtual bool CanResize()
    {
        return IsVisible && !IsLocked;
    }

    public virtual void Move(double deltaX, double deltaY)
    {
        if (!CanMove())
        {
            return;
        }

        X += deltaX;
        Y += deltaY;
    }

    public virtual void Resize(double deltaX, double deltaY, NodeResizeDirection direction)
    {
        if (!CanResize())
        {
            return;
        }

        const double minSize = 1.0;
        var originalX = X;
        var originalY = Y;
        var originalWidth = Width;
        var originalHeight = Height;

        var newX = originalX;
        var newY = originalY;
        var newWidth = originalWidth;
        var newHeight = originalHeight;

        switch (direction)
        {
            case NodeResizeDirection.Left:
                newX = originalX + deltaX;
                newWidth = originalWidth - deltaX;
                break;
            case NodeResizeDirection.Right:
                newWidth = originalWidth + deltaX;
                break;
            case NodeResizeDirection.Top:
                newY = originalY + deltaY;
                newHeight = originalHeight - deltaY;
                break;
            case NodeResizeDirection.Bottom:
                newHeight = originalHeight + deltaY;
                break;
            case NodeResizeDirection.TopLeft:
                newX = originalX + deltaX;
                newWidth = originalWidth - deltaX;
                newY = originalY + deltaY;
                newHeight = originalHeight - deltaY;
                break;
            case NodeResizeDirection.TopRight:
                newWidth = originalWidth + deltaX;
                newY = originalY + deltaY;
                newHeight = originalHeight - deltaY;
                break;
            case NodeResizeDirection.BottomLeft:
                newX = originalX + deltaX;
                newWidth = originalWidth - deltaX;
                newHeight = originalHeight + deltaY;
                break;
            case NodeResizeDirection.BottomRight:
                newWidth = originalWidth + deltaX;
                newHeight = originalHeight + deltaY;
                break;
        }

        if (newWidth < minSize)
        {
            newWidth = minSize;
            if (direction is NodeResizeDirection.Left or NodeResizeDirection.TopLeft or NodeResizeDirection.BottomLeft)
            {
                newX = originalX + (originalWidth - newWidth);
            }
        }

        if (newHeight < minSize)
        {
            newHeight = minSize;
            if (direction is NodeResizeDirection.Top or NodeResizeDirection.TopLeft or NodeResizeDirection.TopRight)
            {
                newY = originalY + (originalHeight - newHeight);
            }
        }

        X = newX;
        Y = newY;
        Width = newWidth;
        Height = newHeight;

        UpdatePinsForResize(originalWidth, originalHeight, newWidth, newHeight);
    }

    private void UpdatePinsForResize(double oldWidth, double oldHeight, double newWidth, double newHeight)
    {
        if (Pins is not { Count: > 0 })
        {
            return;
        }

        var widthDenominator = Math.Abs(oldWidth) < 0.001 ? 1.0 : oldWidth;
        var heightDenominator = Math.Abs(oldHeight) < 0.001 ? 1.0 : oldHeight;

        foreach (var pin in Pins)
        {
            var ratioX = pin.X / widthDenominator;
            var ratioY = pin.Y / heightDenominator;

            switch (pin.Alignment)
            {
                case PinAlignment.Left:
                    pin.X = 0.0;
                    pin.Y = ratioY * newHeight;
                    break;
                case PinAlignment.Right:
                    pin.X = newWidth;
                    pin.Y = ratioY * newHeight;
                    break;
                case PinAlignment.Top:
                    pin.X = ratioX * newWidth;
                    pin.Y = 0.0;
                    break;
                case PinAlignment.Bottom:
                    pin.X = ratioX * newWidth;
                    pin.Y = newHeight;
                    break;
                default:
                    pin.X = ratioX * newWidth;
                    pin.Y = ratioY * newHeight;
                    break;
            }

            pin.OnMoved();
        }
    }

    public virtual void OnCreated()
    {
        Created?.Invoke(this, new NodeCreatedEventArgs(this));
    }

    public virtual void OnRemoved()
    {
        Removed?.Invoke(this, new NodeRemovedEventArgs(this));
    }

    public virtual void OnMoved()
    {
        Moved?.Invoke(this, new NodeMovedEventArgs(this, _x, _y));
    }

    public virtual void OnSelected()
    {
        Selected?.Invoke(this, new NodeSelectedEventArgs(this));
    }

    public virtual void OnDeselected()
    {
        Deselected?.Invoke(this, new NodeDeselectedEventArgs(this));
    }

    public virtual void OnResized()
    {
        Resized?.Invoke(this, new NodeResizedEventArgs(this, _x, _y, _width, _height));
    }
}
