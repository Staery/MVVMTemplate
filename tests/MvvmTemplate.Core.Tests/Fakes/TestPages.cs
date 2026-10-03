using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.Tests.Fakes;

/// <summary>A page that records its navigation callbacks.</summary>
internal class RecordingPage(string title) : PageViewModel(title)
{
    public List<string> Calls { get; } = [];

    public object? LastParameter { get; private set; }

    public override void OnNavigatedTo(object? parameter)
    {
        LastParameter = parameter;
        Calls.Add("to");
    }

    public override void OnNavigatedFrom() => Calls.Add("from");
}

internal sealed class FirstPage() : RecordingPage("First");

internal sealed class SecondPage() : RecordingPage("Second");

/// <summary>Never registered, to test the error path.</summary>
internal sealed class UnregisteredPage() : RecordingPage("Unregistered");
