using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class DrawingNode : TemplatedControl
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<DrawingNode, IDrawingNode?>(nameof(DrawingSource));

    public static readonly StyledProperty<Control?> InputSourceProperty = 
        AvaloniaProperty.Register<DrawingNode, Control?>(nameof(InputSource));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty = 
        AvaloniaProperty.Register<DrawingNode, Canvas?>(nameof(AdornerCanvas));

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    public Control? InputSource
    {
        get => GetValue(InputSourceProperty);
        set => SetValue(InputSourceProperty, value);
    }

    public Canvas? AdornerCanvas
    {
        get => GetValue(AdornerCanvasProperty);
        set => SetValue(AdornerCanvasProperty, value);
    }
}
