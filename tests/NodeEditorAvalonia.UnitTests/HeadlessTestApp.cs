using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

[assembly: AvaloniaTestApplication(typeof(NodeEditor.UnitTests.HeadlessTestAppBuilder))]

namespace NodeEditor.UnitTests;

public static class HeadlessTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<HeadlessTestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public sealed partial class HeadlessTestApp : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
