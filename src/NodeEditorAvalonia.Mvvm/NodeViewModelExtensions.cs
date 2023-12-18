using System.Collections.ObjectModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

public static class NodeViewModelExtensions
{
    public static IPin AddPin(this NodeViewModel node, double x, double y, double width, double height, PinAlignment alignment = PinAlignment.None, string? name = null)
    {
        var pin = new PinViewModel
        {
            Name = name,
            Parent = node,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Alignment = alignment
        };

        node.Pins ??= new ObservableCollection<IPin>();
        node.Pins.Add(pin);

        return pin;
    }
}
