using System.Collections.ObjectModel;
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
                    AddNodeToDrawing(drawing, node, point);
                }
            }

            return true;
        }

        var dataTransfer = e.DataTransfer;
        if (dataTransfer is null)
        {
            return false;
        }

        if (TryGetTemplate(dataTransfer, out var template))
        {
            if (bExecute)
            {
                var node = drawing.Clone(template.Template);
                if (node is not null)
                {
                    AddNodeToDrawing(drawing, node, point);
                }
            }

            return true;
        }

        var text = dataTransfer.TryGetText();
        if (text is not null)
        {
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

        var files = dataTransfer.TryGetFiles();
        if (files is { Length: > 0 })
        {
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

    private static bool TryGetTemplate(IDataTransfer dataTransfer, out INodeTemplate template)
    {
        foreach (var item in dataTransfer.Items)
        {
            foreach (var format in item.Formats)
            {
                if (item.TryGetRaw(format) is INodeTemplate nodeTemplate)
                {
                    template = nodeTemplate;
                    return true;
                }
            }
        }

        template = null!;
        return false;
    }

    private static void AddNodeToDrawing(IDrawingNode drawing, INode node, Point point)
    {
        drawing.Nodes ??= new ObservableCollection<INode>();
        node.Parent = drawing;

        var deltaX = point.X - node.X;
        var deltaY = point.Y - node.Y;

        if (node.CanMove())
        {
            node.Move(deltaX, deltaY);
        }
        else
        {
            node.X += deltaX;
            node.Y += deltaY;
        }

        drawing.Nodes.Add(node);
        node.OnCreated();
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
