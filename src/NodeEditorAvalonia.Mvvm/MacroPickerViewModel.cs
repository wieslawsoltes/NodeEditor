using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NodeEditor.Mvvm;

[ObservableObject]
public partial class MacroPickerViewModel
{
    private readonly ObservableCollection<MacroDefinition> _macros;
    [ObservableProperty] private ObservableCollection<MacroDefinition> _filteredMacros;
    [ObservableProperty] private string _filter = string.Empty;
    [ObservableProperty] private MacroDefinition? _selectedMacro;

    public MacroPickerViewModel(IEnumerable<MacroDefinition> macros)
    {
        _macros = new ObservableCollection<MacroDefinition>(macros ?? Enumerable.Empty<MacroDefinition>());
        _filteredMacros = new ObservableCollection<MacroDefinition>(_macros);
        RunSelectedCommand = new RelayCommand(RunSelected, CanRunSelected);
        CloseCommand = new RelayCommand(RequestCloseInternal);

        if (_filteredMacros.Count > 0)
        {
            _selectedMacro = _filteredMacros[0];
        }
    }

    public IReadOnlyList<MacroDefinition> Macros => _macros;

    public RelayCommand RunSelectedCommand { get; }

    public RelayCommand CloseCommand { get; }

    public event EventHandler? RequestClose;

    partial void OnFilterChanged(string value)
    {
        ApplyFilter(value);
    }

    partial void OnSelectedMacroChanged(MacroDefinition? value)
    {
        RunSelectedCommand.NotifyCanExecuteChanged();
    }

    private bool CanRunSelected()
    {
        var macro = SelectedMacro;
        if (macro is null)
        {
            return false;
        }

        return macro.Command.CanExecute(macro.CommandParameter);
    }

    private void RunSelected()
    {
        var macro = SelectedMacro;
        if (macro is null)
        {
            return;
        }

        if (macro.Command.CanExecute(macro.CommandParameter))
        {
            macro.Command.Execute(macro.CommandParameter);
            RequestCloseInternal();
        }
    }

    private void RequestCloseInternal()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyFilter(string filterText)
    {
        var filter = filterText?.Trim() ?? string.Empty;

        FilteredMacros.Clear();

        if (string.IsNullOrWhiteSpace(filter))
        {
            foreach (var macro in _macros)
            {
                FilteredMacros.Add(macro);
            }

            SelectedMacro = FilteredMacros.Count > 0 ? FilteredMacros[0] : null;
            return;
        }

        foreach (var macro in _macros)
        {
            if (MatchesFilter(macro, filter))
            {
                FilteredMacros.Add(macro);
            }
        }

        if (SelectedMacro is not null && FilteredMacros.Contains(SelectedMacro))
        {
            return;
        }

        SelectedMacro = FilteredMacros.Count > 0 ? FilteredMacros[0] : null;
    }

    private static bool MatchesFilter(MacroDefinition macro, string filter)
    {
        if (ContainsText(macro.Title, filter))
        {
            return true;
        }

        if (ContainsText(macro.Category, filter))
        {
            return true;
        }

        return ContainsText(macro.Description, filter);
    }

    private static bool ContainsText(string? source, string filter)
    {
        var value = source ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
