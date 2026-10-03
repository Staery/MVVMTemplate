using MvvmTemplate.Core.Abstractions;

namespace MvvmTemplate.Core.Tests.Fakes;

/// <summary>Records every dialog and answers confirmations with <see cref="ConfirmResult"/>.</summary>
internal sealed class FakeDialogService : IDialogService
{
    public bool ConfirmResult { get; set; } = true;

    public List<string> Confirmations { get; } = [];

    public List<string> Errors { get; } = [];

    public List<string> Messages { get; } = [];

    public Task<bool> ConfirmAsync(string title, string message)
    {
        Confirmations.Add(message);
        return Task.FromResult(ConfirmResult);
    }

    public Task ShowInfoAsync(string title, string message)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string title, string message)
    {
        Errors.Add(message);
        return Task.CompletedTask;
    }
}
