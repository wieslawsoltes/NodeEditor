using Avalonia;
using Avalonia.Controls.Primitives;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class Pin : TemplatedControl
{
    public static readonly StyledProperty<IPin?> PinSourceProperty =
        AvaloniaProperty.Register<Pin, IPin?>(nameof(PinSource));

    public static readonly StyledProperty<PinAlignment> AlignmentProperty =
        AvaloniaProperty.Register<Pin, PinAlignment>(nameof(Alignment));

    public static readonly StyledProperty<string?> IdProperty =
        AvaloniaProperty.Register<Pin, string?>(nameof(Id));

    public IPin? PinSource
    {
        get => GetValue(PinSourceProperty);
        set => SetValue(PinSourceProperty, value);
    }

    public PinAlignment Alignment
    {
        get => GetValue(AlignmentProperty);
        set => SetValue(AlignmentProperty, value);
    }

    public string? Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }
}
