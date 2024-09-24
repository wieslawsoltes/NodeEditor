using System.Collections;
using Avalonia;
using Avalonia.Controls.Primitives;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class Toolbox : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> TemplatesSourceProperty =
        AvaloniaProperty.Register<Toolbox, IEnumerable?>(nameof(TemplatesSource));
    
    public static readonly StyledProperty<IDrawingNode?> DrawingProperty = 
        AvaloniaProperty.Register<Toolbox, IDrawingNode?>(nameof(Drawing));

    public IEnumerable? TemplatesSource
    {
        get => GetValue(TemplatesSourceProperty);
        set => SetValue(TemplatesSourceProperty, value);
    }
    
    public IDrawingNode? Drawing
    {
        get => GetValue(DrawingProperty);
        set => SetValue(DrawingProperty, value);
    }
}
