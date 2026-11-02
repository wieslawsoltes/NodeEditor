using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class DrawingNode : TemplatedControl
{
    /// <summary>Identifies the optional zip-tie commands exposed to the drawing's context menu.</summary>
    public static readonly DirectProperty<DrawingNode, IZipTieCommands?> ZipTieCommandsProperty =
        AvaloniaProperty.RegisterDirect<DrawingNode, IZipTieCommands?>(nameof(ZipTieCommands), control => control.ZipTieCommands);

    /// <summary>Gets zip-tie editing commands when the drawing supports them.</summary>
    public IZipTieCommands? ZipTieCommands => DrawingSource as IZipTieCommands;

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

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DrawingSourceProperty)
        {
            RaisePropertyChanged(ZipTieCommandsProperty, change.OldValue as IZipTieCommands, ZipTieCommands);
        }
    }
}
