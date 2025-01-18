using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class Toolbox : TemplatedControl
{    
    public static readonly StyledProperty<IEnumerable<INodeTemplate>?> TemplatesSourceProperty =
        AvaloniaProperty.Register<Toolbox, IEnumerable<INodeTemplate>?>(nameof(TemplatesSource));

    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<Toolbox, IDrawingNode?>(nameof(DrawingSource));

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    public IEnumerable<INodeTemplate>? TemplatesSource
    {
        get => GetValue(TemplatesSourceProperty);
        set => SetValue(TemplatesSourceProperty, value);
    }
}
