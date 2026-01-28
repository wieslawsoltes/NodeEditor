using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Mvvm;

namespace NodeEditorLogic.ViewModels;

public partial class LogicConnectorViewModel : ConnectorViewModel
{
    [ObservableProperty] private bool _isInvalid;
    [ObservableProperty] private bool _isContention;
    [ObservableProperty] private bool _isBus;
    [ObservableProperty] private int _busWidth = 1;
    [ObservableProperty] private string? _statusMessage;

    public LogicConnectorViewModel()
    {
        PropertyChanged += OnConnectorPropertyChanged;
        UpdateBusState();
    }

    private void OnConnectorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Start) || e.PropertyName == nameof(End))
        {
            UpdateBusState();
        }
    }

    private void UpdateBusState()
    {
        var width = 1;
        if (Start is LogicPinViewModel startPin)
        {
            width = Math.Max(width, startPin.BusWidth);
        }

        if (End is LogicPinViewModel endPin)
        {
            width = Math.Max(width, endPin.BusWidth);
        }

        BusWidth = Math.Max(1, width);
        IsBus = BusWidth > 1;
    }
}
