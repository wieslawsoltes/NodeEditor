using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorLogic.Models;

namespace NodeEditorLogic.ViewModels;

public partial class LogicPinViewModel : PinViewModel, IConnectablePin
{
    [ObservableProperty] private LogicPinKind _kind;
    [ObservableProperty] private LogicValue _value = LogicValue.Unknown;
    [ObservableProperty] private LogicValue[] _busValue = new[] { LogicValue.Unknown };

    private bool _suppressSignalSync;
    private bool _syncingDirection;

    public LogicPinViewModel()
    {
        PropertyChanged += OnSelfPropertyChanged;
        base.Direction = Kind == LogicPinKind.Input ? PinDirection.Input : PinDirection.Output;
        EnsureBusValue();
    }

    partial void OnKindChanged(LogicPinKind value)
    {
        if (_syncingDirection)
        {
            return;
        }

        _syncingDirection = true;
        base.Direction = value == LogicPinKind.Input ? PinDirection.Input : PinDirection.Output;
        _syncingDirection = false;
    }

    private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BusWidth))
        {
            EnsureBusValue();
            return;
        }

        if (e.PropertyName == nameof(Direction))
        {
            if (_syncingDirection)
            {
                return;
            }

            _syncingDirection = true;
            var expectedDirection = Kind == LogicPinKind.Input ? PinDirection.Input : PinDirection.Output;
            if (Direction == PinDirection.Bidirectional)
            {
                base.Direction = expectedDirection;
            }
            else
            {
                var desiredKind = Direction == PinDirection.Input ? LogicPinKind.Input : LogicPinKind.Output;
                if (Kind != desiredKind)
                {
                    Kind = desiredKind;
                }
            }

            _syncingDirection = false;
        }
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
