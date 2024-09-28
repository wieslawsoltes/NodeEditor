using System.Collections;
using Avalonia;
using Avalonia.Controls.Primitives;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class Pins : TemplatedControl
{
    public static readonly StyledProperty<INode?> NodeSourceProperty =
        AvaloniaProperty.Register<Pins, INode?>(nameof(NodeSource));

    public INode? NodeSource
    {
        get => GetValue(NodeSourceProperty);
        set => SetValue(NodeSourceProperty, value);
    }
}
