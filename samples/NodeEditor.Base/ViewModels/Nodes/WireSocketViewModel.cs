using Newtonsoft.Json;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels.Nodes;

/// <summary>Content for the six-channel socket panels in the zip-tie sample.</summary>
[JsonObject(MemberSerialization.OptOut)]
public sealed class WireSocketViewModel : ReactiveObject
{
    private string _title = string.Empty;
    private string _subtitle = string.Empty;

    /// <summary>Gets or sets the socket title preserved in drawing snapshots and undo history.</summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>Gets or sets the channel-group caption preserved in drawing snapshots and undo history.</summary>
    public string Subtitle
    {
        get => _subtitle;
        set => this.RaiseAndSetIfChanged(ref _subtitle, value);
    }
}
