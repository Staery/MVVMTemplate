using MvvmTemplate.Core.Models;
using MvvmTemplate.Core.Services;
using MvvmTemplate.Core.Tests.Fakes;
using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.Tests;

public sealed class TodoListViewModelTests
{
    private readonly InMemoryTodoService _service = new(new FixedTimeProvider(new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero)));
    private readonly FakeDialogService _dialogs = new();
    private readonly AppSettings _settings = new();
    private readonly TodoListViewModel _viewModel;

    public TodoListViewModelTests() => _viewModel = new TodoListViewModel(_service, _dialogs, _settings);

    [Fact]
    public async Task Load_shows_all_tasks()
    {
        await SeedAsync(("Write tests", false), ("Ship it", true));

        await _viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(["Write tests", "Ship it"], _viewModel.Items.Select(item => item.Title));
        Assert.Equal(2, _viewModel.TotalCount);
        Assert.Equal(1, _viewModel.ActiveCount);
        Assert.Equal(1, _viewModel.CompletedCount);
        Assert.False(_viewModel.IsEmpty);
    }

    [Fact]
    public async Task Navigating_to_the_page_loads_it_and_applies_a_filter_parameter()
    {
        await SeedAsync(("Open", false), ("Closed", true));

        _viewModel.OnNavigatedTo(TodoFilter.Completed);

        Assert.Equal(TodoFilter.Completed, _viewModel.Filter);
        Assert.Equal(["Closed"], _viewModel.Items.Select(item => item.Title));
    }

    [Fact]
    public async Task Add_creates_a_task_and_resets_the_form()
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);
        _viewModel.NewTitle = "  Buy milk  ";

        await _viewModel.AddCommand.ExecuteAsync(null);

        var item = Assert.Single(_viewModel.Items);
        Assert.Equal("Buy milk", item.Title);
        Assert.Single(await _service.GetAllAsync());
        Assert.Equal(string.Empty, _viewModel.NewTitle);
        Assert.False(_viewModel.HasErrors);
        Assert.False(_viewModel.AddCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_is_disabled_for_an_empty_title(string title)
    {
        _viewModel.NewTitle = "Draft";
        _viewModel.NewTitle = title;

        Assert.False(_viewModel.AddCommand.CanExecute(null));
        Assert.True(_viewModel.HasErrors);
    }

    [Fact]
    public void A_pristine_form_shows_no_errors()
    {
        Assert.False(_viewModel.HasErrors);
        Assert.False(_viewModel.AddCommand.CanExecute(null));
    }

    [Fact]
    public void A_too_long_title_is_reported_as_an_error()
    {
        _viewModel.NewTitle = new string('x', TodoListViewModel.MaxTitleLength + 1);

        var error = Assert.Single(_viewModel.GetErrors(nameof(TodoListViewModel.NewTitle)));
        Assert.Contains("at most 80", error.ErrorMessage);
        Assert.False(_viewModel.AddCommand.CanExecute(null));
    }

    [Fact]
    public void A_title_of_maximum_length_is_valid()
    {
        _viewModel.NewTitle = new string('x', TodoListViewModel.MaxTitleLength);

        Assert.False(_viewModel.HasErrors);
        Assert.True(_viewModel.AddCommand.CanExecute(null));
    }

    [Fact]
    public async Task A_duplicate_title_is_rejected_ignoring_case_and_spaces()
    {
        await SeedAsync(("Call Alice", false));
        await _viewModel.LoadCommand.ExecuteAsync(null);

        _viewModel.NewTitle = " call alice ";

        var error = Assert.Single(_viewModel.GetErrors(nameof(TodoListViewModel.NewTitle)));
        Assert.Contains("already exists", error.ErrorMessage);
        Assert.False(_viewModel.AddCommand.CanExecute(null));
    }

    [Fact]
    public void Fixing_an_invalid_title_clears_the_error()
    {
        _viewModel.NewTitle = new string('x', TodoListViewModel.MaxTitleLength + 1);
        Assert.True(_viewModel.HasErrors);

        _viewModel.NewTitle = "Valid";

        Assert.False(_viewModel.HasErrors);
        Assert.True(_viewModel.AddCommand.CanExecute(null));
    }

    [Theory]
    [InlineData(TodoFilter.All, new[] { "A", "B", "C" })]
    [InlineData(TodoFilter.Active, new[] { "A", "C" })]
    [InlineData(TodoFilter.Completed, new[] { "B" })]
    public async Task Filter_limits_the_visible_tasks(TodoFilter filter, string[] expected)
    {
        await SeedAsync(("A", false), ("B", true), ("C", false));
        await _viewModel.LoadCommand.ExecuteAsync(null);

        _viewModel.Filter = filter;

        Assert.Equal(expected, _viewModel.Items.Select(item => item.Title));
    }

    [Fact]
    public async Task Search_matches_part_of_the_title_ignoring_case()
    {
        await SeedAsync(("Write README", false), ("Fix the build", false), ("Review readme", true));
        await _viewModel.LoadCommand.ExecuteAsync(null);

        _viewModel.SearchText = "readme";

        Assert.Equal(["Write README", "Review readme"], _viewModel.Items.Select(item => item.Title));
    }

    [Fact]
    public async Task Search_and_filter_combine_and_keep_the_original_order()
    {
        await SeedAsync(("Write README", false), ("Fix the build", false), ("Review readme", true));
        await _viewModel.LoadCommand.ExecuteAsync(null);

        _viewModel.SearchText = "readme";
        _viewModel.Filter = TodoFilter.Active;
        Assert.Equal(["Write README"], _viewModel.Items.Select(item => item.Title));

        _viewModel.SearchText = string.Empty;
        _viewModel.Filter = TodoFilter.All;
        Assert.Equal(["Write README", "Fix the build", "Review readme"], _viewModel.Items.Select(item => item.Title));
    }

    [Fact]
    public async Task EmptyMessage_tells_an_empty_list_from_an_empty_filter()
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);
        Assert.True(_viewModel.IsEmpty);
        Assert.Contains("No tasks yet", _viewModel.EmptyMessage);

        await SeedAsync(("Open", false));
        await _viewModel.LoadCommand.ExecuteAsync(null);
        _viewModel.Filter = TodoFilter.Completed;

        Assert.True(_viewModel.IsEmpty);
        Assert.Contains("filter", _viewModel.EmptyMessage);
    }

    [Fact]
    public async Task Toggling_a_task_saves_it_and_updates_the_filtered_list()
    {
        await SeedAsync(("A", false), ("B", false));
        await _viewModel.LoadCommand.ExecuteAsync(null);
        _viewModel.Filter = TodoFilter.Active;

        _viewModel.Items[0].IsDone = true;

        Assert.True((await _service.GetAllAsync())[0].IsDone);
        Assert.Equal(["B"], _viewModel.Items.Select(item => item.Title));
        Assert.Equal(1, _viewModel.CompletedCount);
        Assert.True(_viewModel.ClearCompletedCommand.CanExecute(null));
    }

    [Fact]
    public async Task Remove_asks_for_confirmation_and_keeps_the_task_when_declined()
    {
        await SeedAsync(("Keep me", false));
        await _viewModel.LoadCommand.ExecuteAsync(null);
        _dialogs.ConfirmResult = false;

        await _viewModel.RemoveCommand.ExecuteAsync(_viewModel.Items[0]);

        Assert.Single(_dialogs.Confirmations);
        Assert.Single(_viewModel.Items);
        Assert.Single(await _service.GetAllAsync());
    }

    [Fact]
    public async Task Remove_deletes_the_task_when_confirmed()
    {
        await SeedAsync(("Delete me", false), ("Stay", false));
        await _viewModel.LoadCommand.ExecuteAsync(null);

        await _viewModel.RemoveCommand.ExecuteAsync(_viewModel.Items[0]);

        Assert.Contains("Delete me", Assert.Single(_dialogs.Confirmations));
        Assert.Equal(["Stay"], _viewModel.Items.Select(item => item.Title));
        Assert.Equal(["Stay"], (await _service.GetAllAsync()).Select(item => item.Title));
    }

    [Fact]
    public async Task Remove_skips_the_confirmation_when_it_is_turned_off()
    {
        await SeedAsync(("Delete me", false));
        await _viewModel.LoadCommand.ExecuteAsync(null);
        _settings.ConfirmDeletion = false;

        await _viewModel.RemoveCommand.ExecuteAsync(_viewModel.Items[0]);

        Assert.Empty(_dialogs.Confirmations);
        Assert.True(_viewModel.IsEmpty);
    }

    [Fact]
    public async Task ClearCompleted_removes_only_completed_tasks()
    {
        await SeedAsync(("A", true), ("B", false), ("C", true));
        await _viewModel.LoadCommand.ExecuteAsync(null);

        await _viewModel.ClearCompletedCommand.ExecuteAsync(null);

        Assert.Equal(["B"], _viewModel.Items.Select(item => item.Title));
        Assert.Equal(["B"], (await _service.GetAllAsync()).Select(item => item.Title));
        Assert.False(_viewModel.ClearCompletedCommand.CanExecute(null));
    }

    [Fact]
    public async Task ClearCompleted_is_disabled_when_nothing_is_completed()
    {
        await SeedAsync(("A", false));
        await _viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(_viewModel.ClearCompletedCommand.CanExecute(null));
    }

    private async Task SeedAsync(params (string Title, bool IsDone)[] items)
    {
        foreach (var (title, isDone) in items)
        {
            var item = await _service.AddAsync(title);
            if (isDone)
            {
                await _service.UpdateAsync(item with { IsDone = true });
            }
        }
    }
}
