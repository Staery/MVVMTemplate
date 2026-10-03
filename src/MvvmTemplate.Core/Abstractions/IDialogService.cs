namespace MvvmTemplate.Core.Abstractions;

/// <summary>
/// User interaction that view models need but must not implement themselves.
/// The WPF project provides the real implementation; tests use a fake.
/// </summary>
/// <remarks>
/// The methods are asynchronous so that an implementation can show custom, non-blocking dialogs
/// (overlays, flyouts) without changing the view models that call them.
/// </remarks>
public interface IDialogService
{
    /// <summary>Asks a yes/no question. Returns <see langword="true"/> if the user confirmed.</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>Shows an informational message.</summary>
    Task ShowInfoAsync(string title, string message);

    /// <summary>Shows an error message.</summary>
    Task ShowErrorAsync(string title, string message);
}
