using System.Collections;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls;

public class Connectors : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ConnectorsSourceProperty =
        AvaloniaProperty.Register<Connectors, IEnumerable?>(nameof(ConnectorsSource));
    
    public IEnumerable? ConnectorsSource
    {
        get => GetValue(ConnectorsSourceProperty);
        set => SetValue(ConnectorsSourceProperty, value);
    }
}
