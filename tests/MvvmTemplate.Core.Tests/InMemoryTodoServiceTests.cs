using MvvmTemplate.Core.Services;
using MvvmTemplate.Core.Tests.Fakes;

namespace MvvmTemplate.Core.Tests;

public sealed class InMemoryTodoServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 4, 12, 30, 0, TimeSpan.Zero);
    private readonly InMemoryTodoService _service = new(new FixedTimeProvider(Now));

    [Fact]
    public async Task Add_creates_an_open_task_with_a_trimmed_title()
    {
        var item = await _service.AddAsync("  Plan the week ");

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("Plan the week", item.Title);
        Assert.False(item.IsDone);
        Assert.Equal(Now, item.CreatedAt);
        Assert.Equal([item], await _service.GetAllAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Add_rejects_an_empty_title(string title)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(title));
    }

    [Fact]
    public async Task Update_replaces_the_stored_task()
    {
        var item = await _service.AddAsync("Draft");

        await _service.UpdateAsync(item with { Title = "Final", IsDone = true });

        var stored = Assert.Single(await _service.GetAllAsync());
        Assert.Equal("Final", stored.Title);
        Assert.True(stored.IsDone);
    }

    [Fact]
    public async Task Update_of_an_unknown_task_throws()
    {
        var item = await _service.AddAsync("Draft");
        await _service.RemoveAsync(item.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(item));
    }

    [Fact]
    public async Task Remove_reports_whether_the_task_existed()
    {
        var item = await _service.AddAsync("Temporary");

        Assert.True(await _service.RemoveAsync(item.Id));
        Assert.False(await _service.RemoveAsync(item.Id));
        Assert.Empty(await _service.GetAllAsync());
    }

    [Fact]
    public async Task GetAll_returns_a_snapshot()
    {
        await _service.AddAsync("First");
        var snapshot = await _service.GetAllAsync();

        await _service.AddAsync("Second");

        Assert.Single(snapshot);
        Assert.Equal(2, (await _service.GetAllAsync()).Count);
    }

    [Fact]
    public async Task Sample_data_contains_open_and_completed_tasks()
    {
        var items = await InMemoryTodoService.CreateWithSampleData(TimeProvider.System).GetAllAsync();

        Assert.Equal(3, items.Count);
        Assert.Contains(items, item => item.IsDone);
        Assert.Contains(items, item => !item.IsDone);
    }
}
