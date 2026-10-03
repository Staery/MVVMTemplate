using Microsoft.Extensions.DependencyInjection;
using MvvmTemplate.Core.Models;
using MvvmTemplate.Core.Navigation;
using MvvmTemplate.Core.Services;
using MvvmTemplate.Core.Tests.Fakes;
using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.Tests;

public sealed class HomeViewModelTests
{
    [Fact]
    public async Task Load_summarises_the_tasks()
    {
        var service = new InMemoryTodoService(TimeProvider.System);
        await service.AddAsync("One");
        var done = await service.AddAsync("Two");
        await service.UpdateAsync(done with { IsDone = true });
        await service.AddAsync("Three");
        await service.AddAsync("Four");
        var home = new HomeViewModel(service, new NavigationService(new ServiceCollection().BuildServiceProvider()), TimeProvider.System);

        await home.LoadCommand.ExecuteAsync(null);

        Assert.Equal(4, home.TotalCount);
        Assert.Equal(3, home.ActiveCount);
        Assert.Equal(1, home.CompletedCount);
        Assert.Equal(25, home.CompletionPercent);
        Assert.False(home.IsBusy);
    }

    [Fact]
    public void CompletionPercent_is_zero_without_tasks()
    {
        var home = CreateWithClock(TimeProvider.System);

        Assert.Equal(0, home.CompletionPercent);
    }

    [Theory]
    [InlineData(3, "Good night")]
    [InlineData(9, "Good morning")]
    [InlineData(14, "Good afternoon")]
    [InlineData(21, "Good evening")]
    public void Greeting_depends_on_the_time_of_day(int hour, string expected)
    {
        var home = CreateWithClock(new FixedTimeProvider(new DateTimeOffset(2026, 1, 15, hour, 0, 0, TimeSpan.Zero)));

        Assert.Equal(expected, home.Greeting);
    }

    [Fact]
    public void OpenActiveTasks_opens_the_task_list_with_the_active_filter()
    {
        using var services = TestServices.Create();
        var navigation = services.GetRequiredService<INavigationService>();
        navigation.NavigateTo<HomeViewModel>();
        var home = (HomeViewModel)navigation.CurrentPage!;

        home.OpenActiveTasksCommand.Execute(null);

        var tasks = Assert.IsType<TodoListViewModel>(navigation.CurrentPage);
        Assert.Equal(TodoFilter.Active, tasks.Filter);
        Assert.All(tasks.Items, item => Assert.False(item.IsDone));
    }

    [Fact]
    public void OpenSettings_opens_the_settings_page()
    {
        using var services = TestServices.Create();
        var navigation = services.GetRequiredService<INavigationService>();
        navigation.NavigateTo<HomeViewModel>();

        ((HomeViewModel)navigation.CurrentPage!).OpenSettingsCommand.Execute(null);

        Assert.IsType<SettingsViewModel>(navigation.CurrentPage);
    }

    private static HomeViewModel CreateWithClock(TimeProvider clock) =>
        new(new InMemoryTodoService(clock), new NavigationService(new ServiceCollection().BuildServiceProvider()), clock);
}
