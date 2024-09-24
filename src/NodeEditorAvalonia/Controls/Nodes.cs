using System.Collections;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls;

public class Nodes : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> NodesSourceProperty =
        AvaloniaProperty.Register<Nodes, IEnumerable?>(nameof(NodesSource));
    
    public IEnumerable? NodesSource
    {
        get => GetValue(NodesSourceProperty);
        set => SetValue(NodesSourceProperty, value);
    }
}
