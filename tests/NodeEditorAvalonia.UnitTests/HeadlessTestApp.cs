using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(NodeEditor.UnitTests.HeadlessTestAppBuilder))]

namespace NodeEditor.UnitTests;

public static class HeadlessTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<HeadlessTestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public sealed class HeadlessTestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new StyleInclude(new Uri("avares://NodeEditorAvalonia/"))
        {
            Source = new Uri("avares://NodeEditorAvalonia/Themes/NodeEditorTheme.axaml")
        });
    }
}
