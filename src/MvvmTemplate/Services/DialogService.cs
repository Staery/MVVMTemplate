using System.Windows;
using MvvmTemplate.Core.Abstractions;

namespace MvvmTemplate.Services;

/// <summary>
/// <see cref="IDialogService"/> based on <see cref="MessageBox"/>, owned by the main window when it is visible.
/// Replace it with custom dialog windows or in-window overlays without touching the view models.
/// </summary>
internal sealed class DialogService : IDialogService
{
    private static Window? Owner => Application.Current?.MainWindow is { IsVisible: true } window ? window : null;

    public Task<bool> ConfirmAsync(string title, string message) =>
        Task.FromResult(Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);

    public Task ShowInfoAsync(string title, string message)
    {
        Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string title, string message)
    {
        Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    private static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image) =>
        Owner is { } owner
            ? MessageBox.Show(owner, message, title, buttons, image)
            : MessageBox.Show(message, title, buttons, image);
}
