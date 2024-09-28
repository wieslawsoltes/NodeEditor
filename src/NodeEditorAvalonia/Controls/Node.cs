using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using NodeEditor.Model;

namespace NodeEditor.Controls;

[PseudoClasses(":selected")]
public class Node : ContentControl
{
    public static readonly StyledProperty<INode?> NodeSourceProperty =
        AvaloniaProperty.Register<Node, INode?>(nameof(NodeSource));

    public INode? NodeSource
    {
        get => GetValue(NodeSourceProperty);
        set => SetValue(NodeSourceProperty, value);
    }
}
