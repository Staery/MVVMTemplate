using MvvmTemplate.Core.Models;

namespace MvvmTemplate.Core.Services;

/// <summary>Data access for the sample "Tasks" feature. Swap the implementation for a database or a web API.</summary>
public interface ITodoService
{
    /// <summary>Returns all tasks, oldest first.</summary>
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a new, not yet completed task.</summary>
    /// <exception cref="ArgumentException"><paramref name="title"/> is empty.</exception>
    Task<TodoItem> AddAsync(string title, CancellationToken cancellationToken = default);

    /// <summary>Replaces the stored task that has the same <see cref="TodoItem.Id"/>.</summary>
    /// <exception cref="KeyNotFoundException">No task with this id exists.</exception>
    Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default);

    /// <summary>Deletes a task. Returns <see langword="false"/> if it did not exist.</summary>
    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
