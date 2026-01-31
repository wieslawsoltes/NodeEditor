using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace NodeEditor.Controls;

public class EditableTextBlock : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<EditableTextBlock, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<EditableTextBlock, string?>(nameof(Placeholder));

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<EditableTextBlock, bool>(nameof(IsEditing));

    public static readonly StyledProperty<bool> AcceptsReturnProperty =
        AvaloniaProperty.Register<EditableTextBlock, bool>(nameof(AcceptsReturn));

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<EditableTextBlock, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<EditableTextBlock, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

    private TextBox? _editor;
    private string? _originalText;

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public bool AcceptsReturn
    {
        get => GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public EditableTextBlock()
    {
        AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_editor is not null)
        {
            _editor.LostFocus -= EditorLostFocus;
            _editor.KeyDown -= EditorKeyDown;
        }

        _editor = e.NameScope.Find<TextBox>("PART_Editor");

        if (_editor is not null)
        {
            _editor.LostFocus += EditorLostFocus;
            _editor.KeyDown += EditorKeyDown;
        }

        if (IsEditing)
        {
            FocusEditor();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsEditingProperty && change.NewValue is bool isEditing)
        {
            if (isEditing)
            {
                _originalText ??= Text;
                FocusEditor();
            }
            else
            {
                _originalText = null;
            }
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            BeginEdit();
            e.Handled = true;
        }
    }

    private void EditorLostFocus(object? sender, RoutedEventArgs e)
    {
        CommitEdit();
    }

    private void EditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CancelEdit();
            e.Handled = true;
            return;
        }

        if (!AcceptsReturn && e.Key == Key.Enter)
        {
            CommitEdit();
            e.Handled = true;
            return;
        }

        if (AcceptsReturn && e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            CommitEdit();
            e.Handled = true;
        }
    }

    private void BeginEdit()
    {
        if (IsEditing)
        {
            return;
        }

        _originalText = Text;
        IsEditing = true;
    }

    private void CommitEdit()
    {
        if (!IsEditing)
        {
            return;
        }

        IsEditing = false;
    }

    private void CancelEdit()
    {
        if (!IsEditing)
        {
            return;
        }

        Text = _originalText;
        IsEditing = false;
    }

    private void FocusEditor()
    {
        if (_editor is null)
        {
            return;
        }

        _editor.Focus();
        _editor.SelectAll();
    }
}
