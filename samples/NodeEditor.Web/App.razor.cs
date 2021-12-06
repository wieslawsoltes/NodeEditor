using Avalonia.ReactiveUI;
using Avalonia.Web.Blazor;

namespace NodeEditor.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        WebAppBuilder.Configure<NodeEditorDemo.App>()
            .UseReactiveUI()
            .SetupWithSingleViewLifetime();
    }
}