using MvvmTemplate.Core.Models;

namespace MvvmTemplate.Core.Services;

/// <summary>Thread-safe, in-memory <see cref="ITodoService"/>. Data lives as long as the process.</summary>
public sealed class InMemoryTodoService(TimeProvider timeProvider) : ITodoService
{
    private readonly List<TodoItem> _items = [];
    private readonly object _gate = new();

    /// <summary>Creates a service pre-filled with a few tasks, so the sample page is not empty on first launch.</summary>
    public static InMemoryTodoService CreateWithSampleData(TimeProvider timeProvider)
    {
        var service = new InMemoryTodoService(timeProvider);
        service.Seed("Explore the project structure", isDone: true);
        service.Seed("Add your first page", isDone: false);
        service.Seed("Replace the in-memory service with real storage", isDone: false);
        return service;
    }

    public Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<TodoItem>>([.. _items]);
        }
    }

    public Task<TodoItem> AddAsync(string title, CancellationToken cancellationToken = default) =>
        Task.FromResult(Seed(title, isDone: false));

    public Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_gate)
        {
            var index = _items.FindIndex(existing => existing.Id == item.Id);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Task {item.Id} does not exist.");
            }

            _items[index] = item with { Title = item.Title.Trim() };
        }

        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_items.RemoveAll(item => item.Id == id) > 0);
        }
    }

    private TodoItem Seed(string title, bool isDone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var item = new TodoItem(Guid.NewGuid(), title.Trim(), isDone, timeProvider.GetUtcNow());
        lock (_gate)
        {
            _items.Add(item);
        }

        return item;
    }
}
