using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;
using NodeEditorDemo.ViewModels;

namespace NodeEditorDemo.Behaviors
{
    public class TemplatesListBoxDropHandler : DropHandlerBase
    {
        private bool Validate<T>(ListBox listBox, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute) where T : NodeTemplateViewModel
        {
            if (sourceContext is not T sourceItem
                || targetContext is not MainWindowViewModel vm
                || vm.Templates is null
                || listBox.GetVisualAt(e.GetPosition(listBox)) is not IControl targetControl
                || targetControl.DataContext is not T targetItem)
            {
                return false;
            }

            var sourceIndex = vm.Templates.IndexOf(sourceItem);
            var targetIndex = vm.Templates.IndexOf(targetItem);

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
                    MoveItem(vm.Templates, sourceIndex, targetIndex);
                }
                return true;
            }

            if (e.DragEffects == DragDropEffects.Link)
            {
                if (bExecute)
                {
                    SwapItem(vm.Templates, sourceIndex, targetIndex);
                }
                return true;
            }

            return false;
        }
        
        public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            if (e.Source is IControl && sender is ListBox listBox)
            {
                return Validate<NodeTemplateViewModel>(listBox, e, sourceContext, targetContext, false);
            }
            return false;
        }

        public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            if (e.Source is IControl && sender is ListBox listBox)
            {
                return Validate<NodeTemplateViewModel>(listBox, e, sourceContext, targetContext, true);
            }
            return false;
        }
    }
}
