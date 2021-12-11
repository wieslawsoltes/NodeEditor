using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingDropHandler : DefaultDropHandler
{
    public static readonly StyledProperty<IControl?> RelativeToProperty =
        AvaloniaProperty.Register<DrawingDropHandler, IControl?>(nameof(RelativeTo));

    public IControl? RelativeTo
    {
        get => GetValue(RelativeToProperty) as Control;
        set => SetValue(RelativeToProperty, value);
    }

    private bool Validate(IDrawingNode drawing, DragEventArgs e, bool bExecute)
    {
        var point = GetPosition(RelativeTo, e);

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
                        var node = template.Build?.Invoke(point.X, point.Y);
                        if (node is { })
                        {
                            node.Parent = drawing;
                            drawing.Nodes?.Add(node);
                        }
                    }
                    return true;
                }
            }
        }

        if (e.Data.Contains(DataFormats.FileNames))
        {
            // ReSharper disable once UnusedVariable
            var files = e.Data.GetFileNames()?.ToArray();
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
            return Validate(drawing, e, false);
        }

        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (targetContext is IDrawingNode drawing)
        {
            return Validate(drawing, e, true);
        }

        return false;
    }
}
