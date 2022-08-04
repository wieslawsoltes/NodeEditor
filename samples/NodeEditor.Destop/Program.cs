using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.ReactiveUI;

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
            .With(new Win32PlatformOptions
            {
                UseCompositor = false,
                UseDeferredRendering = true
            })
            .With(new X11PlatformOptions
            {
                UseCompositor = false,
                UseDeferredRendering = true
            })
            .With(new AvaloniaNativePlatformOptions
            {
                UseCompositor = false,
                UseDeferredRendering = true
            })
            .LogToTrace()
            .UseReactiveUI()
            .UseSkia();
}
