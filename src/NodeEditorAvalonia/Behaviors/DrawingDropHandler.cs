using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingDropHandler : DefaultDropHandler
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<DrawingDropHandler, IDrawingNode?>(nameof(DrawingSource));

    public static readonly StyledProperty<Control?> RelativeToProperty =
        AvaloniaProperty.Register<DrawingDropHandler, Control?>(nameof(RelativeTo));

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    public Control? RelativeTo
    {
        get => GetValue(RelativeToProperty);
        set => SetValue(RelativeToProperty, value);
    }

    private bool Validate(IDrawingNode drawing, object? sender, DragEventArgs e, object? sourceContext, bool bExecute)
    {
        var relativeTo = RelativeTo ?? sender as Control;
        if (relativeTo is null)
        {
            return false;
        }
        var point = GetPosition(relativeTo, e);

        if (relativeTo is DrawingNode { DrawingSource: not null } drawingNode)
        {
            point = SnapHelper.Snap(point, drawingNode.DrawingSource.Settings.SnapX, drawingNode.DrawingSource.Settings.SnapY, drawingNode.DrawingSource.Settings.EnableSnap);
        }

        if (sourceContext is INodeTemplate directTemplate)
        {
            if (bExecute)
            {
                var node = drawing.Clone(directTemplate.Template);
                if (node is not null)
                {
                    node.Parent = drawing;
                    node.Move(point.X, point.Y);
                    drawing.Nodes?.Add(node);
                    node.OnCreated();
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
                        if (node is not null)
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

        if (e.Data.Contains(DataFormats.Text))
        {
            var text = e.Data.GetText();
            if (text is null)
            {
                return false;
            }

            if (drawing is IDrawingDropTarget dropTarget && dropTarget.CanDropText(text, point))
            {
                if (bExecute)
                {
                    dropTarget.DropText(text, point);
                }

                return true;
            }

            return false;
        }

        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles()?.ToArray();
            if (files is null || files.Length == 0)
            {
                return false;
            }

            if (drawing is IDrawingDropTarget dropTarget && dropTarget.CanDropFiles(files, point))
            {
                if (bExecute)
                {
                    dropTarget.DropFiles(files, point);
                }

                return true;
            }

            return false;
        }

        return false;
    }

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        var drawing = targetContext as IDrawingNode ?? DrawingSource;
        if (drawing is null)
        {
            return false;
        }

        return Validate(drawing, sender, e, sourceContext, false);
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        var drawing = targetContext as IDrawingNode ?? DrawingSource;
        if (drawing is null)
        {
            return false;
        }

        if (drawing is IUndoRedoHost host)
        {
            host.BeginUndoBatch();
        }

        try
        {
            return Validate(drawing, sender, e, sourceContext, true);
        }
        finally
        {
            if (drawing is IUndoRedoHost endHost)
            {
                endHost.EndUndoBatch();
            }
        }
    }
}
