using System;
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
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions()
            {
                UseDeferredRendering = true
            })
            .With(new X11PlatformOptions()
            {
                UseDeferredRendering = true
            })
            .With(new AvaloniaNativePlatformOptions()
            {
                UseDeferredRendering = true
            })
            .LogToTrace()
            .UseReactiveUI()
            .UseSkia();
}
