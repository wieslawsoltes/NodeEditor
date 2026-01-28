using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditorLogic.Models;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicClockNodeViewModel : LogicNodeContentViewModel
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private int _period = 2;
    [ObservableProperty] private int _counter;
    [ObservableProperty] private LogicValue _state = LogicValue.Low;
    [ObservableProperty] private int _highTicks = 1;

    partial void OnPeriodChanged(int value)
    {
        if (value < 1)
        {
            Period = 1;
            return;
        }

        if (HighTicks > value)
        {
            HighTicks = value;
        }
    }

    partial void OnHighTicksChanged(int value)
    {
        if (value < 1)
        {
            HighTicks = 1;
            return;
        }

        if (value > Period)
        {
            HighTicks = Period;
        }
    }
}
