using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingDropHandler : DefaultDropHandler
{
    public static readonly StyledProperty<Control?> RelativeToProperty =
        AvaloniaProperty.Register<DrawingDropHandler, Control?>(nameof(RelativeTo));

    public Control? RelativeTo
    {
        get => GetValue(RelativeToProperty);
        set => SetValue(RelativeToProperty, value);
    }

    private bool Validate(IDrawingNode drawing, object? sender, DragEventArgs e, bool bExecute)
    {
        var relativeTo = RelativeTo ?? sender as Control;
        if (relativeTo is null)
        {
            return false;
        }
        var point = GetPosition(relativeTo, e);

        if (relativeTo is DrawingNode drawingNode)
        {
            point = SnapHelper.Snap(point, drawingNode.SnapX, drawingNode.SnapY, drawingNode.EnableSnap);
        }

        if (e.Data.Contains(DataFormats.Text))
        {
            var text = e.Data.GetText();

            if (bExecute)
            {
                if (text is { })
                {
                    // TODO: text
                }
            }

            return true;
        }

        foreach (var format in e.Data.GetDataFormats())
        {
            var data = e.Data.Get(format);

            switch (data)
            {
                case INodeTemplate template:
                {
                    if (bExecute)
                    {
                        var node = drawing.Clone(template.Template);
                        if (node is { })
                        {
                            node.Parent = drawing;
                            node.Move(point.X, point.Y);
                            drawing.Nodes?.Add(node);
                            node.OnCreated();
                        }
                    }
                    return true;
                }
            }
        }

        if (e.Data.Contains(DataFormats.Files))
        {
            // ReSharper disable once UnusedVariable
            var files = e.Data.GetFiles()?.ToArray();
            if (bExecute)
            {
                // TODO: files, point.X, point.Y
            }

            return true;
        }

        return false;
    }

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (targetContext is IDrawingNode drawing)
        {
            return Validate(drawing, sender, e, false);
        }

        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (targetContext is IDrawingNode drawing)
        {
            return Validate(drawing, sender, e, true);
        }

        return false;
    }
}
