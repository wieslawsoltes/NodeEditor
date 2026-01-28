using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class InsertTemplateOnDoubleTappedBehavior : Behavior<ListBoxItem>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty = 
        AvaloniaProperty.Register<InsertTemplateOnDoubleTappedBehavior, IDrawingNode?>(nameof(DrawingSource));

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is not null)
        {
            AssociatedObject.DoubleTapped += DoubleTapped; 
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is not null)
        {
            AssociatedObject.DoubleTapped -= DoubleTapped; 
        }
    }

    private void DoubleTapped(object? sender, RoutedEventArgs args)
    {
        if (AssociatedObject is { DataContext: INodeTemplate template } && DrawingSource is { } drawing)
        {
            if (drawing is IUndoRedoHost host)
            {
                host.BeginUndoBatch();
            }

            var node = drawing.Clone(template.Template);
            if (node is not null)
            {
                AddNodeToDrawing(drawing, node);
            }

            if (drawing is IUndoRedoHost endHost)
            {
                endHost.EndUndoBatch();
            }
        }
    }

    private static void AddNodeToDrawing(IDrawingNode drawing, INode node)
    {
        drawing.Nodes ??= new ObservableCollection<INode>();
        node.Parent = drawing;

        var deltaX = -node.X;
        var deltaY = -node.Y;

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
}
