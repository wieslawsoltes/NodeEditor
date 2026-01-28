using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicBusOutputNodeViewModel : LogicNodeContentViewModel
{
    [ObservableProperty] private string _label = "BUS";
    [ObservableProperty] private int _busWidth = 4;
    [ObservableProperty] private string _observedText = "X";

    partial void OnBusWidthChanged(int value)
    {
        var clamped = value < 1 ? 1 : value > 16 ? 16 : value;
        if (clamped != value)
        {
            BusWidth = clamped;
        }
    }
}
