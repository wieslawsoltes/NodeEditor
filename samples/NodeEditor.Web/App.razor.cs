using Avalonia.ReactiveUI;
using Avalonia.Web.Blazor;

namespace NodeEditor.Web;

public partial class App
{
    static App()
    {
        NodeEditorDemo.App.EnableInputOutput = true;
        NodeEditorDemo.App.EnableMainMenu = true;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        WebAppBuilder.Configure<NodeEditorDemo.App>()
            .UseReactiveUI()
            .SetupWithSingleViewLifetime();
    }
}
