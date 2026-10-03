namespace MvvmTemplate.Core.Models;

/// <summary>
/// Application-wide preferences, registered as a singleton so every page sees the same values.
/// </summary>
/// <remarks>Kept in memory for the template; persist it (JSON, registry, …) if your app needs to.</remarks>
public sealed class AppSettings
{
    /// <summary>Ask for confirmation before a task is deleted.</summary>
    public bool ConfirmDeletion { get; set; } = true;
}
