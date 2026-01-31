using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NodeEditor.Mvvm;

namespace NodeEditorLogic.Views;

public partial class MacroPickerWindow : Window
{
    private MacroPickerViewModel? _viewModel;

    public MacroPickerWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.RequestClose -= OnRequestClose;
        }

        _viewModel = DataContext as MacroPickerViewModel;

        if (_viewModel is not null)
        {
            _viewModel.RequestClose += OnRequestClose;
        }

        base.OnDataContextChanged(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.RequestClose -= OnRequestClose;
        }

        base.OnClosed(e);
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        Close();
    }

    private void OnMacroDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.RunSelectedCommand.CanExecute(null) == true)
        {
            _viewModel.RunSelectedCommand.Execute(null);
        }
    }
}
