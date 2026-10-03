using System.Reflection;
using System.Runtime.InteropServices;
using MvvmTemplate.Core.Models;

namespace MvvmTemplate.Core.ViewModels;

/// <summary>Edits the shared <see cref="AppSettings"/> and shows information about the application.</summary>
public sealed class SettingsViewModel(AppSettings settings) : PageViewModel("Settings")
{
    /// <summary>Forwards to <see cref="AppSettings.ConfirmDeletion"/> and raises change notifications.</summary>
    public bool ConfirmDeletion
    {
        get => settings.ConfirmDeletion;
        set => SetProperty(settings.ConfirmDeletion, value, settings, static (s, v) => s.ConfirmDeletion = v);
    }

    public string Version { get; } =
        typeof(SettingsViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]
        ?? "1.0.0";

    public string Runtime { get; } = RuntimeInformation.FrameworkDescription;
}
