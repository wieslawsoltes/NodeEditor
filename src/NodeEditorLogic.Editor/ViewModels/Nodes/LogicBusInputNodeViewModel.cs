using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditorLogic.Services;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicBusInputNodeViewModel : LogicNodeContentViewModel
{
    [ObservableProperty] private string _label = "BUS";
    [ObservableProperty] private int _busWidth = 4;
    [ObservableProperty] private int _busValue;

    public int MaxValue => GetMaxValue(BusWidth);

    partial void OnBusWidthChanged(int value)
    {
        var clamped = ClampWidth(value);
        if (clamped != value)
        {
            BusWidth = clamped;
            return;
        }

        if (BusValue > MaxValue)
        {
            BusValue = MaxValue;
        }

        OnPropertyChanged(nameof(MaxValue));

        if (HostNode is not null)
        {
            LogicNodeFactory.RefreshBusInputPins(HostNode, this);
        }
    }

    partial void OnBusValueChanged(int value)
    {
        var clamped = value;
        if (value < 0)
        {
            clamped = 0;
        }
        else if (value > MaxValue)
        {
            clamped = MaxValue;
        }

        if (clamped != value)
        {
            BusValue = clamped;
        }
    }

    private static int ClampWidth(int width)
    {
        if (width < 1)
        {
            return 1;
        }

        return width > 16 ? 16 : width;
    }

    private static int GetMaxValue(int width)
    {
        var clamped = ClampWidth(width);
        return (1 << clamped) - 1;
    }
}
