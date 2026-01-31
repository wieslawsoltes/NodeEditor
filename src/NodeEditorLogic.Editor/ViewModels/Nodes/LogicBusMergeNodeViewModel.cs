using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditorLogic.Services;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicBusMergeNodeViewModel : LogicNodeContentViewModel
{
    [ObservableProperty] private int _bitCount = 4;

    partial void OnBitCountChanged(int value)
    {
        var clamped = value < 1 ? 1 : value > 16 ? 16 : value;
        if (clamped != value)
        {
            BitCount = clamped;
            return;
        }

        if (HostNode is not null)
        {
            LogicNodeFactory.RefreshBusMergePins(HostNode, this);
        }
    }
}
