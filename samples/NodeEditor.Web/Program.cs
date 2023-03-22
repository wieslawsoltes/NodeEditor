using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using NodeEditorDemo;

[assembly:SupportedOSPlatform("browser")]

internal partial class Program
{
    static Program()
    {
        App.EnableInputOutput = true;
        App.EnableMainMenu = true;
    }

    private static async Task Main(string[] args) 
        => await BuildAvaloniaApp().StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
           => AppBuilder.Configure<App>();
}
