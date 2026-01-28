using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorLogic.Models;

namespace NodeEditorLogic.ViewModels;

public partial class LogicPinViewModel : PinViewModel, IConnectablePin
{
    [ObservableProperty] private LogicPinKind _kind;
    [ObservableProperty] private LogicValue _value = LogicValue.Unknown;
    [ObservableProperty] private int _busWidth = 1;
    [ObservableProperty] private LogicValue[] _busValue = new[] { LogicValue.Unknown };

    private bool _suppressSignalSync;

    public PinDirection Direction => Kind == LogicPinKind.Input ? PinDirection.Input : PinDirection.Output;

    partial void OnBusWidthChanged(int value)
    {
        var clamped = value < 1 ? 1 : value;
        if (clamped != value)
        {
            BusWidth = clamped;
            return;
        }

        EnsureBusValue();
    }

    partial void OnBusValueChanged(LogicValue[] value)
    {
        if (_suppressSignalSync)
        {
            return;
        }

        if (BusWidth <= 1 && value.Length > 0)
        {
            _suppressSignalSync = true;
            Value = value[0];
            _suppressSignalSync = false;
        }
    }

    partial void OnValueChanged(LogicValue value)
    {
        if (_suppressSignalSync)
        {
            return;
        }

        if (BusWidth <= 1)
        {
            _suppressSignalSync = true;
            BusValue = new[] { value };
            _suppressSignalSync = false;
        }
    }

    private void EnsureBusValue()
    {
        if (BusWidth <= 1)
        {
            BusValue = new[] { Value };
            return;
        }

        if (BusValue.Length == BusWidth)
        {
            return;
        }

        var next = new LogicValue[BusWidth];
        for (var i = 0; i < next.Length; i++)
        {
            next[i] = i < BusValue.Length ? BusValue[i] : LogicValue.Unknown;
        }

        BusValue = next;
    }
}
