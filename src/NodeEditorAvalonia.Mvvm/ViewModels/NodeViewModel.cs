using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.ViewModels;

[ObservableObject]
public partial class NodeViewModel : INode
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private INode? _parent;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    [ObservableProperty] private object? _content;
    [ObservableProperty] private IList<IPin>? _pins;

    public virtual bool CanSelect()
    {
        return true;
    }

    public virtual bool CanRemove()
    {
        return true;
    }

    public virtual bool CanMove()
    {
        return true;
    }

    public virtual bool CanResize()
    {
        return true;
    }

    public virtual void Move(double deltaX, double deltaY)
    {
        X += deltaX;
        Y += deltaY;
    }

    public virtual void Resize(double deltaX, double deltaY, NodeResizeDirection direction)
    {
        // TODO: Resize
    }
}
