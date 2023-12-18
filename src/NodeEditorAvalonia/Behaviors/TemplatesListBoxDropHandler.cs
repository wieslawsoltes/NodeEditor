using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class TemplatesListBoxDropHandler : DropHandlerBase
{
    private bool Validate<T>(ListBox listBox, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute) where T : INodeTemplate
    {
        if (sourceContext is not T sourceItem
            || targetContext is not INodeTemplatesHost nodeTemplatesHost
            || nodeTemplatesHost.Templates is null
            || listBox.GetVisualAt(e.GetPosition(listBox)) is not Control targetControl
            || targetControl.DataContext is not T targetItem)
        {
            return false;
        }

        var sourceIndex = nodeTemplatesHost.Templates.IndexOf(sourceItem);
        var targetIndex = nodeTemplatesHost.Templates.IndexOf(targetItem);

        if (sourceIndex < 0 || targetIndex < 0)
        {
            return false;
        }

        if (e.DragEffects == DragDropEffects.Copy)
        {
            return false;
        }

        if (e.DragEffects == DragDropEffects.Move)
        {
            if (bExecute)
            {
                MoveItem(nodeTemplatesHost.Templates, sourceIndex, targetIndex);
            }
            return true;
        }

        if (e.DragEffects == DragDropEffects.Link)
        {
            if (bExecute)
            {
                SwapItem(nodeTemplatesHost.Templates, sourceIndex, targetIndex);
            }
            return true;
        }

        return false;
    }
        
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is Control && sender is ListBox listBox)
        {
            return Validate<INodeTemplate>(listBox, e, sourceContext, targetContext, false);
        }
        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is Control && sender is ListBox listBox)
        {
            return Validate<INodeTemplate>(listBox, e, sourceContext, targetContext, true);
        }
        return false;
    }
}
