using ReactiveUI;

namespace NodeEditorDemo.ViewModels.Nodes;

/// <summary>Content for the six-channel socket panels in the zip-tie sample.</summary>
public sealed class WireSocketViewModel : ReactiveObject
{
    private string _title = string.Empty;
    private string _subtitle = string.Empty;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        set => this.RaiseAndSetIfChanged(ref _subtitle, value);
    }
}
