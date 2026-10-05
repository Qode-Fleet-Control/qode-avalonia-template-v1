using System.ComponentModel;
using AvaloniaApp.ViewModels;

namespace AvaloniaApp.Tests;

/// <summary>Plain unit tests: the view model needs no UI thread.</summary>
public class MainViewModelTests
{
    [Fact]
    public void Greeting_raises_PropertyChanged()
    {
        var vm = new MainViewModel();
        var raised = new List<string?>();
        ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.Greeting = "Hi";

        Assert.Equal([nameof(MainViewModel.Greeting)], raised);
    }
}
