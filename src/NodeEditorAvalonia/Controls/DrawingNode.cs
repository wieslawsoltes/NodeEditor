using Avalonia;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls
{
    public class DrawingNode : TemplatedControl
    {
        public static readonly AttachedProperty<bool> IsEditModeProperty = 
            AvaloniaProperty.RegisterAttached<IAvaloniaObject, bool>("IsEditMode", typeof(DrawingNode), true, true);

        public static bool GetIsEditMode(IAvaloniaObject obj)
        {
            return obj.GetValue(IsEditModeProperty);
        }

        public static void SetIsEditMode(IAvaloniaObject obj, bool value)
        {
            obj.SetValue(IsEditModeProperty, value);
        }
    }
}
