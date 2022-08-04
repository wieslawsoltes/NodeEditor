using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.ViewModels;

[ObservableObject]
public partial class PinViewModel : IPin
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private INode? _parent;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    [ObservableProperty] private PinAlignment _alignment;

    public virtual bool CanConnect()
    {
        return true;
    }

    public virtual bool CanDisconnect()
    {
        return true;
    }
}
