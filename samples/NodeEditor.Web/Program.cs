using Avalonia;
using Avalonia.Web;
using NodeEditorDemo;

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
