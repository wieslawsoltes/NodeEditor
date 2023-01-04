using System.Runtime.Versioning;
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

    private static void Main(string[] args)
        => BuildAvaloniaApp().SetupBrowserApp("out");

    public static AppBuilder BuildAvaloniaApp()
           => AppBuilder.Configure<App>();
}
