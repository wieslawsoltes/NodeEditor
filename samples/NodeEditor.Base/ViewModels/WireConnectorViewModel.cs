using NodeEditor.Mvvm;

namespace NodeEditorDemo.ViewModels;

/// <summary>A sample connector with a serialized channel number for its wire color.</summary>
public sealed class WireConnectorViewModel : ConnectorViewModel
{
    private int _channel;

    public int Channel
    {
        get => _channel;
        set => SetProperty(ref _channel, value);
    }
}
