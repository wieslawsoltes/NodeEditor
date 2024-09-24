using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace NodeEditor.Controls;

[PseudoClasses(":selected")]
public class Node : ContentControl
{
    public static readonly StyledProperty<IEnumerable?> PinsSourceProperty =
        AvaloniaProperty.Register<Node, IEnumerable?>(nameof(PinsSource));
    
    public IEnumerable? PinsSource
    {
        get => GetValue(PinsSourceProperty);
        set => SetValue(PinsSourceProperty, value);
    }
}
