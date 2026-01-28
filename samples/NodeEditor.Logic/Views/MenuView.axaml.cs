using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using NodeEditor.Controls;

namespace NodeEditorLogic.Views;

public partial class MenuView : UserControl
{
    public static readonly StyledProperty<NodeZoomBorder?> ZoomControlProperty =
        AvaloniaProperty.Register<MenuView, NodeZoomBorder?>(nameof(ZoomControl));

    public MenuView()
    {
        InitializeComponent();
    }

    public NodeZoomBorder? ZoomControl
    {
        get => GetValue(ZoomControlProperty);
        set => SetValue(ZoomControlProperty, value);
    }

    private void OnDragRegionPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (IsInteractiveSource(e.Source))
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.BeginMoveDrag(e);
            e.Handled = true;
        }
    }

    private static bool IsInteractiveSource(object? source)
    {
        if (source is not Visual visual)
        {
            return false;
        }

        if (visual is Control control && IsInteractiveControl(control, includeFocusable: true))
        {
            return true;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is Control ancestorControl && IsInteractiveControl(ancestorControl, includeFocusable: false))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInteractiveControl(Control control, bool includeFocusable)
    {
        if (control is Thumb
            || control is TextBox
            || control is ToggleButton
            || control is Button
            || control is ComboBox
            || control is Slider
            || control is SelectingItemsControl
            || control is MenuItem)
        {
            return true;
        }

        return includeFocusable && control.Focusable;
    }
}
