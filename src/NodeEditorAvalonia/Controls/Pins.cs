using System.Collections;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls;

public class Pins : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> PinsSourceProperty =
        AvaloniaProperty.Register<Pins, IEnumerable?>(nameof(PinsSource));
    
    public IEnumerable? PinsSource
    {
        get => GetValue(PinsSourceProperty);
        set => SetValue(PinsSourceProperty, value);
    }
}
