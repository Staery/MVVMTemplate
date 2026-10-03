namespace MvvmTemplate.Core.Models;

/// <summary>A single task of the sample feature. Immutable: changes go through <see cref="Services.ITodoService"/>.</summary>
/// <param name="Id">Stable identifier.</param>
/// <param name="Title">Short description shown in the list.</param>
/// <param name="IsDone">Whether the task has been completed.</param>
/// <param name="CreatedAt">When the task was created.</param>
public sealed record TodoItem(Guid Id, string Title, bool IsDone, DateTimeOffset CreatedAt);
