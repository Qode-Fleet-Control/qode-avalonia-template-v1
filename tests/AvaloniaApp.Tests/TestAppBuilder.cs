using Avalonia;
using Avalonia.Headless;

// Every [AvaloniaFact] / [AvaloniaTheory] in this assembly runs on one headless Avalonia
// app built here — the real App (its styles and resources), on the headless platform
// instead of a desktop window system. No display server, no GPU.
[assembly: AvaloniaTestApplication(typeof(AvaloniaApp.Tests.TestAppBuilder))]

namespace AvaloniaApp.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
