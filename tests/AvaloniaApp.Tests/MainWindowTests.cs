using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvaloniaApp.ViewModels;
using AvaloniaApp.Views;

namespace AvaloniaApp.Tests;

/// <summary>UI tests: the real MainWindow, shown on the headless platform.</summary>
public class MainWindowTests
{
    private static (MainWindow Window, MainViewModel Vm, TextBox Box) Show()
    {
        var vm = new MainViewModel();
        var window = new MainWindow { DataContext = vm };
        window.Show();
        var box = Assert.IsType<TextBox>(window.Content);
        return (window, vm, box);
    }

    [AvaloniaFact]
    public void Shows_the_greeting()
    {
        var (window, _, box) = Show();

        Assert.True(window.IsVisible);
        Assert.Equal("Welcome to Avalonia!", box.Text);
    }

    [AvaloniaFact]
    public void Typing_into_the_box_updates_the_view_model()
    {
        var (window, vm, box) = Show();

        box.Focus();
        box.SelectAll();
        window.KeyTextInput("Hello from a headless test");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Hello from a headless test", vm.Greeting);
    }

    [AvaloniaFact]
    public void Changing_the_view_model_updates_the_box()
    {
        var (_, vm, box) = Show();

        vm.Greeting = "Changed in code";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Changed in code", box.Text);
    }
}
