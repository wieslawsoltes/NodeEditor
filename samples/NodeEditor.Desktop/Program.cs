using System;
using System.Runtime.InteropServices;
using Avalonia;

namespace NodeEditorDemo;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    static Program()
    {
        App.EnableInputOutput = true;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
        {
            App.EnableMainMenu = true;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            App.EnableMainMenu = true;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            App.EnableMainMenu = false;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            App.EnableMainMenu = false;
        }
        else
        {
            App.EnableMainMenu = true;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseSkia();
}
